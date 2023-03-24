namespace MyC_02
{
    using System.IO;
    using OpenCvSharp;

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    string sourceFile;
                    string targetFolder;
                    string filename;
                    
                    // Копируем изображения с левой камеры, в папку для дальшейшей обработки
                    {
                        // Определяем исходный файл и папку назначения для копирования изображения
                        sourceFile = @"_ExportImage\LeftCamTexture_01.png";
                        targetFolder = @"_ExportImage\OutputNormalMap\";

                        // Получаем имя файла из пути
                        filename = Path.GetFileName(sourceFile);

                        // Если файл уже существует в папке назначения, то удаляем его
                        if (File.Exists(Path.Combine(targetFolder, filename)))
                        {
                            File.Delete(Path.Combine(targetFolder, filename));
                        }

                        // Копируем исходный файл в папку назначения
                        File.Copy(sourceFile, Path.Combine(targetFolder, filename));
                    }

                    // Копируем изображения с правой камеры, в папку для дальшейшей обработки
                    {
                        // Определяем исходный файл и папку назначения для копирования изображения
                        sourceFile = @"_ExportImage\RightCamTexture_01.png";
                        targetFolder = @"_ExportImage\OutputNormalMap\";

                        // Получаем имя файла из пути
                        filename = Path.GetFileName(sourceFile);

                        // Если файл уже существует в папке назначения, то удаляем его
                        if (File.Exists(Path.Combine(targetFolder, filename)))
                        {
                            File.Delete(Path.Combine(targetFolder, filename));
                        }

                        // Копируем исходный файл в папку назначения
                        File.Copy(sourceFile, Path.Combine(targetFolder, filename));
                    }

                    // Загружаем изображения из папки назначения в объекты Mat с градациями серого
                    Mat imgL = new Mat(@"_ExportImage\OutputNormalMap\LeftCamTexture_01.png", ImreadModes.Grayscale);
                    Mat imgR = new Mat(@"_ExportImage\OutputNormalMap\RightCamTexture_01.png", ImreadModes.Grayscale);

                    // Изменяем размер изображений, для лучшей работы алгоритма создания карты глубин
                    Cv2.Resize(imgL, imgL, new Size(576, 324));
                    Cv2.Resize(imgR, imgR, new Size(576, 324));

                    // Создаем новый объект StereoBM для вычисления карты глубины
                    using (StereoBM stereo = StereoBM.Create(numDisparities: 16 * 1, blockSize: 31))
                    {
                        Mat disparity = new Mat();
                        stereo.Compute(imgL, imgR, disparity); // Вычисляем карту глубины

                        // Сохраняем карту глубины в файл
                        Cv2.ImWrite(@"_ImportDephMap\NewDephMap.png", disparity);
                    }

                    // Временная задержка в 10 миллисекунд. Так, программа срабатывает примерно 100 раз в секунду
                    System.Threading.Thread.Sleep(10);

                    /*
                    // Вывод сине-красной карты, для отладки
                    {
                        Mat doble_inp = new Mat(@"D:\Unity Progect\Diplom Project 01\Assets\Images\Output\DephMap.png", ImreadModes.Grayscale); // Load the saved depth map for viewing

                        Mat img_color = new Mat();
                        Cv2.ApplyColorMap(doble_inp, img_color, ColormapTypes.Jet); // Convert the grayscale image to a color image with a color map from blue to red

                        Cv2.ImShow("Image", img_color); // Display the image
                        Cv2.WaitKey(0); // Wait for the user to close the window
                    }
                    */
                }
                catch { } // При ошибке открытия или сохранения файлов, не останавливаем программу
            }

        }

    }
}
