using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Tools
{
    /// <summary>
    /// 이미지 데이터를 처리하여 프레임을 추출하는 유틸리티 클래스입니다. 단일 이미지 및 애니메이션 GIF 등 멀티프레임 이미지를 지원합니다.
    /// </summary>
    public class ImageExtractor
    {
        /// <summary>
        /// 메모리의 이미지 데이터에서 모든 프레임을 추출합니다.
        /// </summary>
        /// <param name="imageData">이미지 바이트 배열</param>
        /// <returns>추출된 SKImage 프레임 목록</returns>
        public static List<SKImage> ProcessImageFromMemory(byte[] imageData)
        {
            var frames = new List<SKImage>();

            try
            {
                using var stream = new SKMemoryStream(imageData);
                using var codec = SKCodec.Create(stream);

                if (codec == null)
                    throw new Exception("제공된 이미지 데이터에서 코덱을 생성할 수 없습니다.");

                var info = codec.Info;
                var frameCount = codec.FrameCount;

                if (frameCount > 1)
                {
                    for (int i = 0; i < frameCount; i++)
                    {
                        using var bitmap = new SKBitmap(info);
                        if (codec.GetFrameInfo(i, out var frameInfo))
                        {
                            var options = new SKCodecOptions(i);
                            var result = codec.GetPixels(info, bitmap.GetPixels(), options);
                            if (result == SKCodecResult.Success)
                            {
                                var image = SKImage.FromBitmap(bitmap);
                                if (image != null) frames.Add(image);
                            }
                        }
                    }
                }
                else
                {
                    using var bitmap = new SKBitmap(info);
                    if (codec.GetPixels(info, bitmap.GetPixels()) == SKCodecResult.Success)
                    {
                        var image = SKImage.FromBitmap(bitmap);
                        if (image != null) frames.Add(image);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return frames;
        }
    }
}
