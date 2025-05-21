```csharp
using Microsoft.Extensions.Logging;
// Assuming SharpCV is available via NuGet: SharpCV
// And its dependencies (OpenCV native binaries) are correctly set up.
using SharpCV; // Main namespace for SharpCV
using System;
using System.IO; // For MemoryStream, if converting between byte arrays and Mat

namespace AIService.Infrastructure.Utils
{
    public static class ImageProcessingUtils
    {
        // Logger cannot be injected into static class directly.
        // If logging is needed, methods could accept an ILogger instance,
        // or this could be an instance class registered in DI.
        // For simplicity, omitting logger here or assuming it's passed if critical.

        /// <summary>
        /// Resizes an image represented by a SharpCV Mat object.
        /// </summary>
        /// <param name="image">The input image (Mat).</param>
        /// <param name="targetWidth">The target width.</param>
        /// <param name="targetHeight">The target height.</param>
        /// <param name="interpolation">Interpolation method to use (e.g., INTER.LINEAR).</param>
        /// <returns>A new Mat object representing the resized image.</returns>
        public static Mat Resize(Mat image, int targetWidth, int targetHeight, INTER interpolation = INTER.LINEAR)
        {
            if (image == null || image.IsDisposed)
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or disposed.");
            if (targetWidth <= 0 || targetHeight <= 0)
                throw new ArgumentOutOfRangeException($"Target dimensions ({targetWidth}x{targetHeight}) must be positive.");

            var newSize = new Size(targetWidth, targetHeight);
            Mat resizedImage = Cv2.Resize(image, newSize, 0, 0, interpolation);
            return resizedImage;
        }

        /// <summary>
        /// Normalizes pixel values of an image. Commonly scales to [0, 1] or [-1, 1].
        /// This example scales to [0, 1] assuming input is 8-bit unsigned.
        /// </summary>
        /// <param name="image">The input image (Mat), expected to be CV_8U or similar.</param>
        /// <returns>A new Mat object (CV_32F) with pixel values normalized to [0, 1].</returns>
        public static Mat Normalize(Mat image)
        {
             if (image == null || image.IsDisposed)
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or disposed.");

            // Convert to float32 for normalization
            Mat floatImage = new Mat();
            image.ConvertTo(floatImage, MatType.CV_32F);

            // Normalize to [0, 1] range
            Mat normalizedImage = floatImage / 255.0; // Assuming input was 0-255 range
            
            // If a different normalization is needed (e.g., mean/std subtraction):
            // Cv2.Normalize(image, normalizedImage, alpha: 0, beta: 1, norm_type: NormTypes.MinMax, dtype: MatType.CV_32F);
            // Or use mean and stddev:
            // Cv2.MeanStdDev(image, out var mean, out var stddev);
            // normalizedImage = (image - mean) / stddev; // conceptual
            
            return normalizedImage;
        }

        /// <summary>
        /// Converts image color space.
        /// </summary>
        /// <param name="image">The input image (Mat).</param>
        /// <param name="conversionCode">E.g., ColorConversionCodes.BGR2RGB.</param>
        /// <returns>A new Mat object with converted color space.</returns>
        public static Mat ConvertColor(Mat image, ColorConversionCodes conversionCode)
        {
            if (image == null || image.IsDisposed)
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or disposed.");

            Mat convertedImage = new Mat();
            Cv2.CvtColor(image, convertedImage, conversionCode);
            return convertedImage;
        }

        /// <summary>
        /// Loads an image from a byte array.
        /// </summary>
        /// <param name="imageBytes">Byte array containing the image data.</param>
        /// <param name="flags">Flags for image loading (e.g., ImreadModes.Color).</param>
        /// <returns>A Mat object representing the loaded image.</returns>
        public static Mat ImDecode(byte[] imageBytes, ImreadModes flags = ImreadModes.Color)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentNullException(nameof(imageBytes), "Image byte array cannot be null or empty.");
            
            Mat image = Cv2.ImDecode(imageBytes, flags);
            if (image == null || image.Empty())
                throw new Exception("Failed to decode image from byte array. Ensure valid image format.");
            return image;
        }
        
        /// <summary>
        /// Loads an image from a stream.
        /// </summary>
        /// <param name="imageStream">Stream containing the image data.</param>
        /// <param name="flags">Flags for image loading (e.g., ImreadModes.Color).</param>
        /// <returns>A Mat object representing the loaded image.</returns>
        public static Mat ImDecode(Stream imageStream, ImreadModes flags = ImreadModes.Color)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));
            if (!imageStream.CanRead)
                throw new ArgumentException("Image stream must be readable.", nameof(imageStream));

            byte[] imageBytes;
            if (imageStream is MemoryStream ms)
            {
                imageBytes = ms.ToArray();
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    imageStream.CopyTo(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }
            }
            return ImDecode(imageBytes, flags);
        }


        /// <summary>
        /// Encodes an image (Mat) into a byte array (e.g., .jpg, .png).
        /// </summary>
        /// <param name="image">The input image (Mat).</param>
        /// <param name="extension">The desired output format (e.g., ".jpg", ".png").</param>
        /// <param name="encodeParams">Optional encoding parameters.</param>
        /// <returns>A byte array containing the encoded image.</returns>
        public static byte[] ImEncode(Mat image, string extension, int[] encodeParams = null)
        {
            if (image == null || image.IsDisposed)
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or disposed.");
            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentNullException(nameof(extension), "Image extension cannot be null or empty.");

            bool result = Cv2.ImEncode(extension, image, out byte[] buffer, encodeParams);
            if (!result || buffer == null || buffer.Length == 0)
                throw new Exception($"Failed to encode image to format: {extension}");
            return buffer;
        }

        // Add other utilities as needed: crop, rotate, convert to tensor-like array, etc.
        // Example: Convert Mat to a flat float array (e.g. for ONNX/TF input)
        // This requires careful handling of channels (BGR vs RGB) and layout (HWC vs CHW).
        // SharpCV Mat data is usually HWC.
        public static float[] ToFlatFloatArray(Mat image, bool normalize = true, bool bgrToRgb = false, bool hwcToChw = false)
        {
            if (image == null || image.IsDisposed)
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or disposed.");

            Mat processedImage = image.Clone(); // Work on a clone

            if (bgrToRgb && processedImage.Channels() == 3)
            {
                Mat temp = new Mat();
                Cv2.CvtColor(processedImage, temp, ColorConversionCodes.BGR2RGB);
                processedImage.Dispose();
                processedImage = temp;
            }

            if (processedImage.Type() != MatType.CV_32FC(processedImage.Channels()))
            {
                Mat temp = new Mat();
                processedImage.ConvertTo(temp, MatType.CV_32FC(processedImage.Channels()), normalize ? 1.0/255.0 : 1.0);
                processedImage.Dispose();
                processedImage = temp;
            }
            else if (normalize) // Already float, just normalize if not done by ConvertTo
            {
                 // Assuming it wasn't normalized yet. If ConvertTo did it, this might double-normalize.
                 // This depends on the source image's original depth.
                 // It's safer if Normalize() is called explicitly before this method if needed.
                 // For simplicity, this example assumes if it's already float, it might not be normalized.
                Mat temp = processedImage / 255.0; // if original was 0-255 and converted to float without scaling
                processedImage.Dispose();
                processedImage = temp;
            }


            int rows = processedImage.Rows;
            int cols = processedImage.Cols;
            int channels = processedImage.Channels();
            float[] floatArray = new float[rows * cols * channels];

            if (hwcToChw) // Convert from HWC (Height, Width, Channel) to CHW (Channel, Height, Width)
            {
                unsafe // Requires AllowUnsafeBlocks in .csproj
                {
                    float* pData = (float*)processedImage.Data;
                    int hw = rows * cols;
                    for (int c = 0; c < channels; ++c)
                    {
                        for (int h = 0; h < rows; ++h)
                        {
                            for (int w = 0; w < cols; ++w)
                            {
                                floatArray[c * hw + h * cols + w] = pData[(h * cols + w) * channels + c];
                            }
                        }
                    }
                }
            }
            else // HWC layout directly
            {
                 if (processedImage.IsContinuous())
                {
                    System.Runtime.InteropServices.Marshal.Copy(processedImage.Data, floatArray, 0, floatArray.Length);
                }
                else // If not continuous, copy row by row
                {
                    unsafe // Requires AllowUnsafeBlocks in .csproj
                    {
                        float* pData = (float*)processedImage.Data;
                        int step = (int)(processedImage.Step() / sizeof(float)); // Elements per row
                        int elemSize1 = (int)processedImage.ElemSize1(); // Bytes per element in a single channel

                        for (int r = 0; r < rows; ++r)
                        {
                            for (int c = 0; c < cols * channels; ++c)
                            {
                                floatArray[r * cols * channels + c] = pData[r * step + c];
                            }
                        }
                    }
                }
            }
            
            processedImage.Dispose();
            return floatArray;
        }
    }
}