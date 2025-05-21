using Microsoft.Extensions.Logging; // Adding logger for potential issues
using SharpCV; // Assuming this is the correct namespace for SharpCV types like Mat, Size, Point, Scalar
using System;
using System.IO;
using static SharpCV.Cv; // For direct access to Cv static methods like imread, imwrite, resize, etc.

namespace AIService.Infrastructure.Utils
{
    public static class ImageProcessingUtils
    {
        // Logger can't be injected into a static class directly.
        // If logging is critical, consider making this an instance class or passing ILogger to methods.
        // For utilities, often exceptions are preferred over logging directly here.

        /// <summary>
        /// Reads an image from a stream.
        /// </summary>
        /// <param name="imageStream">The stream containing image data.</param>
        /// <returns>A SharpCV Mat object representing the image.</returns>
        public static Mat ReadImageFromStream(Stream imageStream)
        {
            if (imageStream == null || imageStream.Length == 0)
                throw new ArgumentNullException(nameof(imageStream), "Image stream cannot be null or empty.");

            try
            {
                byte[] imageBytes;
                using (var memoryStream = new MemoryStream())
                {
                    imageStream.CopyTo(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }
                // cv.imdecode might be more direct if available for streams/byte arrays
                return imdecode(imageBytes, ImreadModes.Color);
            }
            catch (Exception ex)
            {
                // Log using a passed-in logger or rethrow a specific exception
                throw new InvalidOperationException("Failed to read image from stream using SharpCV.", ex);
            }
        }

        /// <summary>
        /// Reads an image from a file path.
        /// </summary>
        /// <param name="filePath">Path to the image file.</param>
        /// <returns>A SharpCV Mat object representing the image.</returns>
        public static Mat ReadImageFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Image file not found.", filePath);

            try
            {
                return imread(filePath, ImreadModes.Color);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read image from file '{filePath}' using SharpCV.", ex);
            }
        }


        /// <summary>
        /// Resizes an image to the specified dimensions.
        /// </summary>
        /// <param name="image">The input image (Mat).</param>
        /// <param name="newWidth">The target width.</param>
        /// <param name="newHeight">The target height.</param>
        /// <param name="interpolation">Interpolation method to use (e.g., InterpolationFlags.Linear).</param>
        /// <returns>A new Mat object representing the resized image.</returns>
        public static Mat ResizeImage(Mat image, int newWidth, int newHeight, InterpolationFlags interpolation = InterpolationFlags.Linear)
        {
            if (image == null || image.empty())
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or empty.");
            if (newWidth <= 0 || newHeight <= 0)
                throw new ArgumentOutOfRangeException("New width and height must be positive.");

            try
            {
                var newSize = new Size(newWidth, newHeight);
                var resizedImage = new Mat();
                resize(image, resizedImage, newSize, 0, 0, interpolation);
                return resizedImage;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to resize image using SharpCV.", ex);
            }
        }

        /// <summary>
        /// Converts an image to a different color space.
        /// E.g., from BGR to RGB, or BGR to Grayscale.
        /// </summary>
        /// <param name="image">The input image (Mat).</param>
        /// <param name="conversionCode">The ColorConversionCodes enum value (e.g., ColorConversionCodes.BGR2RGB).</param>
        /// <returns>A new Mat object in the target color space.</returns>
        public static Mat ConvertColor(Mat image, ColorConversionCodes conversionCode)
        {
            if (image == null || image.empty())
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or empty.");

            try
            {
                var convertedImage = new Mat();
                cvtColor(image, convertedImage, conversionCode);
                return convertedImage;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to convert image color space using SharpCV.", ex);
            }
        }

        /// <summary>
        /// Normalizes pixel values of an image to a specified range (typically 0-1 or -1 to 1 for AI models).
        /// This example performs a simple scaling by a factor (e.g., 1/255.0f).
        /// For more complex normalization (mean subtraction, std dev division), use cv.normalize or manual Mat operations.
        /// </summary>
        /// <param name="image">The input image (Mat), expected to be CV_32F or CV_64F type for scaling.</param>
        /// <param name="scaleFactor">The factor to multiply pixel values by.</param>
        /// <returns>A new Mat object with normalized pixel values.</returns>
        public static Mat NormalizeImageSimple(Mat image, float scaleFactor)
        {
            if (image == null || image.empty())
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or empty.");

            // Ensure the image is of a floating point type for scaling if not already
            Mat floatImage;
            if (image.depth() != MatType.CV_32F && image.depth() != MatType.CV_64F)
            {
                floatImage = new Mat();
                image.convertTo(floatImage, MatType.CV_32F); // Convert to 32-bit float
            }
            else
            {
                floatImage = image.clone(); // Clone to avoid modifying original if it's already float
            }

            try
            {
                // Simple scaling: result = image * scaleFactor
                var normalizedImage = floatImage * scaleFactor;
                return normalizedImage;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to normalize image using SharpCV.", ex);
            }
            finally
            {
                if(floatImage != image) floatImage.Dispose(); // Dispose temporary float image if created
            }
        }

        /// <summary>
        /// Converts an image (Mat) to a float array, typically for AI model input.
        /// Handles channel order (e.g., HWC to CHW if needed, or BGR to RGB).
        /// This is a basic example assuming HWC and direct float conversion.
        /// </summary>
        /// <param name="image">The input image (Mat), expected to be 3-channel (e.g., BGR) and CV_32F type.</param>
        /// <param name="interleaveChannels">If true, output is [c1_p1, c2_p1, c3_p1, c1_p2, ...]. If false (planar), output is [c1_p1, c1_p2, ..., c2_p1, ...]. </param>
        /// <returns>A float array of pixel data.</returns>
        public static float[] ImageToFloatArray(Mat image, bool interleaveChannels = true)
        {
            if (image == null || image.empty())
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or empty.");
            if (image.depth() != MatType.CV_32F)
                throw new ArgumentException("Image must be of type CV_32F for direct float array conversion.", nameof(image));

            int rows = image.rows();
            int cols = image.cols();
            int channels = image.channels();
            float[] floatArray = new float[rows * cols * channels];

            try
            {
                if (interleaveChannels) // HWC format [B,G,R, B,G,R, ...]
                {
                    // SharpCV's Mat.data should provide direct access or a method to get data as array.
                    // This assumes Mat data is already in HWC order if channels > 1.
                    // If SharpCV uses a different internal layout or requires specific access:
                    var matData = image.dataAsFloat(); // Or similar method in SharpCV
                    if (matData.Length != floatArray.Length)
                        throw new InvalidOperationException("Mismatch between Mat data size and expected array size.");
                    Array.Copy(matData, floatArray, matData.Length);
                }
                else // Planar format CHW [B,B,..., G,G,..., R,R,...]
                {
                    var splitChannels = cvtColor(image, ColorConversionCodes.BGR2RGB).split(); // Example: Ensure RGB and split
                    
                    int channelSize = rows * cols;
                    for (int c = 0; c < channels; c++)
                    {
                        var channelData = splitChannels[c].dataAsFloat(); // Get data for one channel
                        Array.Copy(channelData, 0, floatArray, c * channelSize, channelSize);
                        splitChannels[c].Dispose(); // Dispose individual channel Mats
                    }
                }
                return floatArray;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to convert image to float array using SharpCV.", ex);
            }
        }

        /// <summary>
        /// Saves an image (Mat) to a file.
        /// </summary>
        /// <param name="image">The image to save.</param>
        /// <param name="filePath">The path where the image will be saved. Extension determines format (e.g., .png, .jpg).</param>
        /// <param name="parameters">Optional encoding parameters (e.g., JPEG quality).</param>
        public static void SaveImage(Mat image, string filePath, params int[] parameters)
        {
            if (image == null || image.empty())
                throw new ArgumentNullException(nameof(image), "Input image cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                imwrite(filePath, image, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save image to '{filePath}' using SharpCV.", ex);
            }
        }
    }
}