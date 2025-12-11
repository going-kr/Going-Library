using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Going.Basis.Utils
{
    public static class CompressionUtility
    {
        private const int CompressionThreshold = 1024; // 1KB - 이보다 작으면 압축하지 않음

        public class CompressionResult
        {
            public string? Data { get; set; }
            public bool IsCompressed { get; set; }
            public int OriginalSize { get; set; }
            public int ProcessedSize { get; set; }
            public double CompressionRatio => OriginalSize > 0 ? (double)ProcessedSize / OriginalSize : 1.0;
        }

        public class DecompressionResult<T>
        {
            public T? Data { get; set; }
            public bool WasCompressed { get; set; }
            public int OriginalSize { get; set; }
            public int DecompressedSize { get; set; }
        }

        public static CompressionResult CompressJson<T>(T obj)
        {
            try
            {
                // JSON 직렬화
                var jsonString = JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    WriteIndented = false // 압축을 위해 공백 제거
                });

                var originalBytes = Encoding.UTF8.GetBytes(jsonString);
                var originalSize = originalBytes.Length;

                // 임계값보다 작으면 압축하지 않음
                if (originalSize < CompressionThreshold)
                {
                    return new CompressionResult
                    {
                        Data = Convert.ToBase64String(originalBytes),
                        IsCompressed = false,
                        OriginalSize = originalSize,
                        ProcessedSize = originalSize
                    };
                }

                // GZip 압축
                using var outputStream = new MemoryStream();
                using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(originalBytes, 0, originalBytes.Length);
                }

                var compressedBytes = outputStream.ToArray();
                var compressedSize = compressedBytes.Length;

                // 압축 효과가 없으면 원본 사용
                if (compressedSize >= originalSize * 0.9) // 10% 미만 압축 시 원본 사용
                {
                    return new CompressionResult
                    {
                        Data = Convert.ToBase64String(originalBytes),
                        IsCompressed = false,
                        OriginalSize = originalSize,
                        ProcessedSize = originalSize
                    };
                }

                // Base64 인코딩
                var base64String = Convert.ToBase64String(compressedBytes);

                return new CompressionResult
                {
                    Data = base64String,
                    IsCompressed = true,
                    OriginalSize = originalSize,
                    ProcessedSize = base64String.Length
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("JSON 압축 중 오류가 발생했습니다.", ex);
            }
        }

        public static DecompressionResult<T> DecompressJson<T>(string base64Data, bool isCompressed, int originalSize)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64Data);

                if (!isCompressed)
                {
                    // 압축되지 않은 데이터
                    var jsonString = Encoding.UTF8.GetString(bytes);
                    var jsonData = JsonSerializer.Deserialize<T>(jsonString);

                    return new DecompressionResult<T>
                    {
                        Data = jsonData ?? default(T),
                        WasCompressed = false,
                        OriginalSize = originalSize,
                        DecompressedSize = bytes.Length
                    };
                }

                // GZip 압축 해제
                using var inputStream = new MemoryStream(bytes);
                using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
                using var outputStream = new MemoryStream();

                gzipStream.CopyTo(outputStream);
                var decompressedBytes = outputStream.ToArray();

                var decompressedJsonString = Encoding.UTF8.GetString(decompressedBytes);
                var decompressedJsonData = JsonSerializer.Deserialize<T>(decompressedJsonString);

                return new DecompressionResult<T>
                {
                    Data = decompressedJsonData ,
                    WasCompressed = true,
                    OriginalSize = originalSize,
                    DecompressedSize = decompressedBytes.Length
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("JSON 압축 해제 중 오류가 발생했습니다.", ex);
            }
        }

        public static bool ShouldCompress(int dataSize)
        {
            return dataSize >= CompressionThreshold;
        }

        public static string GetCompressionInfo(int originalSize, int processedSize, bool isCompressed)
        {
            if (!isCompressed)
                return "압축되지 않음";

            var ratio = (double)processedSize / originalSize;
            var savedBytes = originalSize - processedSize;
            var savedPercentage = (1 - ratio) * 100;

            return $"압축률: {ratio:P1}, 절약: {savedBytes:N0}bytes ({savedPercentage:F1}%)";
        }

        // TODO: 자주 조회되는 TemplateData 메모리 캐싱 구현
        // TODO: 압축 성능 모니터링 및 통계 수집
    }
}
