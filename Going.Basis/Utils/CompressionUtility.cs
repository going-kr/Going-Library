using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Going.Basis.Utils
{
    /// <summary>
    /// JSON 직렬화 데이터의 GZip 압축/해제 유틸리티.
    /// 임계값(1KB) 이상의 데이터만 압축하며, 압축 효과가 10% 미만이면 원본을 사용한다.
    /// </summary>
    public static class CompressionUtility
    {
        private const int CompressionThreshold = 1024; // 1KB - 이보다 작으면 압축하지 않음

        /// <summary>압축 결과를 담는 클래스</summary>
        public class CompressionResult
        {
            /// <summary>Base64 인코딩된 데이터 (압축 또는 원본)</summary>
            public string? Data { get; set; }
            /// <summary>GZip 압축 적용 여부</summary>
            public bool IsCompressed { get; set; }
            /// <summary>원본 데이터 크기 (바이트)</summary>
            public int OriginalSize { get; set; }
            /// <summary>처리 후 데이터 크기 (바이트)</summary>
            public int ProcessedSize { get; set; }
            /// <summary>압축률 (ProcessedSize / OriginalSize)</summary>
            public double CompressionRatio => OriginalSize > 0 ? (double)ProcessedSize / OriginalSize : 1.0;
        }

        /// <summary>압축 해제 결과를 담는 제네릭 클래스</summary>
        /// <typeparam name="T">역직렬화 대상 타입</typeparam>
        public class DecompressionResult<T>
        {
            /// <summary>역직렬화된 데이터 객체</summary>
            public T? Data { get; set; }
            /// <summary>원본이 압축되어 있었는지 여부</summary>
            public bool WasCompressed { get; set; }
            /// <summary>압축 전 원본 크기 (바이트)</summary>
            public int OriginalSize { get; set; }
            /// <summary>압축 해제 후 크기 (바이트)</summary>
            public int DecompressedSize { get; set; }
        }

        /// <summary>객체를 JSON으로 직렬화한 후 GZip 압축하여 Base64 문자열로 반환한다.</summary>
        /// <typeparam name="T">직렬화할 객체 타입</typeparam>
        /// <param name="obj">직렬화할 객체</param>
        /// <returns>압축 결과 (데이터, 압축 여부, 크기 정보 포함)</returns>
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

        /// <summary>Base64 인코딩된 데이터를 압축 해제하고 JSON 역직렬화하여 객체로 반환한다.</summary>
        /// <typeparam name="T">역직렬화 대상 타입</typeparam>
        /// <param name="base64Data">Base64 인코딩된 데이터 문자열</param>
        /// <param name="isCompressed">데이터가 GZip 압축되어 있는지 여부</param>
        /// <param name="originalSize">압축 전 원본 크기</param>
        /// <returns>압축 해제 및 역직렬화 결과</returns>
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

        /// <summary>데이터 크기가 압축 임계값(1KB) 이상인지 확인한다.</summary>
        /// <param name="dataSize">데이터 크기 (바이트)</param>
        /// <returns>압축이 필요하면 true</returns>
        public static bool ShouldCompress(int dataSize)
        {
            return dataSize >= CompressionThreshold;
        }

        /// <summary>압축 결과에 대한 요약 정보 문자열을 반환한다.</summary>
        /// <param name="originalSize">원본 크기 (바이트)</param>
        /// <param name="processedSize">처리 후 크기 (바이트)</param>
        /// <param name="isCompressed">압축 적용 여부</param>
        /// <returns>압축률과 절약 크기를 포함한 정보 문자열</returns>
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
