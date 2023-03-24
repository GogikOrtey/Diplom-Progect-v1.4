using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System.IO;
using System.Diagnostics;

using System;
using UnityEngine.UI;

using System.Linq;
using TMPro;

public class Screen_Shots_01 : MonoBehaviour
{
    public PlayerController_01 PlayerController_01; // Связь со скриптом, который управляет персонажем

    public RenderTexture LeftCam;       // Ссылки на RenderTexture с правой и левой камеры персонажа
    public RenderTexture RightCam;

    public GameObject DephMapImage;     // Ссылки на экраны для рендера в основном окне карты глубины,
    public GameObject OutLineTexture;   // Сжатой карты глубины
    public GameObject OutMarcerLine;    // И маркерной линии

    public Text speedText;              // Текст, в котором отображается текущая скорость персонажа

    public Image Wall;                  // 4 изображения, отображающие текущие команды для персонажа
    public Image Arr_left;
    public Image Arr_right;
    public Image Em_Stop;

    string fileWayRoot= @"\Resources\"; // Корневая папка с доступом к скрипту, и папкам импорта

    string OpenCVScript;                // 3 пути, к нужным нам папкам, в файловой системе
    string ExportImageWay;
    string ImportDephMapWay;

    Process process;                    // Метод процесса запуска скрипта. Объявляем его здесь, для того, что бы потом легко остановить

    public string newOpenCVScript;      // Дополнительный путь, к скрипу

    //public TextMeshProUGUI debugText;

    private void Awake()
    {
        Screen.SetResolution(1280, 720, true); // Устанавливаем разрешение экрана
        Screen.fullScreen = false; // Устанавливаем режим оконного экрана
    }

    public void Start()
    {
        print("____Start");

        // Тут мы прописываем пути, к папкам экспора, импорта, и к скрипту OpenCV
        if (File.Exists(Directory.GetCurrentDirectory() + @"\Diplom Project 01_Data" + fileWayRoot + @"MyC_02.exe"))
        {
            // Эти пути, если игра - уже билд
            print("Файл существует по пути: " + newOpenCVScript);
            fileWayRoot = Directory.GetCurrentDirectory() + @"\Diplom Project 01_Data" + fileWayRoot;

            OpenCVScript = fileWayRoot + @"MyC_02.exe";
            ExportImageWay = fileWayRoot + @"_ExportImage\";
            ImportDephMapWay = fileWayRoot + @"_ImportDephMap\NewDephMap.png";

            newOpenCVScript = OpenCVScript;
        }
        else
        {
            // И эти пути, если игра ещё запускается в редакторе
            fileWayRoot = Directory.GetCurrentDirectory() + @"\Assets" + fileWayRoot;

            newOpenCVScript = fileWayRoot + @"MyC_02.exe";
            ExportImageWay = fileWayRoot + @"_ExportImage\";
            ImportDephMapWay = fileWayRoot + @"_ImportDephMap\NewDephMap.png";
        }

        //debugText.text = newOpenCVScript;

        // Создаём и запускаем процесс - открытие файла .exe - который является скопмилированным скриптом OpenCV, для генерации карты глубин
        var processStartInfo = new ProcessStartInfo(newOpenCVScript);
        processStartInfo.WorkingDirectory = Path.GetDirectoryName(newOpenCVScript);
        process = Process.Start(processStartInfo);

        // Проставляем значения картинкам на главном экране, соответствующим 4м выходным управляющим методам
        Wall.enabled = false;
        Arr_left.enabled = false;
        Arr_right.enabled = false;
        Em_Stop.enabled = false;

        CreateMarcerLine(); // Рисуем марекрную линию центрального препятствия
    }

    const int allCountRixels = 58; // Всего пикселей в выходном изображении
    int marcerOffset = 3; // Задаю, насколько широкой будет центральная область

    void CreateMarcerLine()
    {
        Texture2D myNewTex = new Texture2D(allCountRixels, 1);

        Color32[] allWhitePixels = Enumerable.Repeat(new Color32(255, 255, 255, 255), myNewTex.width * myNewTex.height).ToArray();
        myNewTex.SetPixels32(allWhitePixels);
        myNewTex.Apply();

        // Задаю центральную зону
        // Если какой-либо образ линий появляется в этой области, значит персонаж обязательно столкнётся с препятствием, если продолжит движение
        for (int i = (allCountRixels / 2) - marcerOffset; i < (allCountRixels / 2) + marcerOffset; i++)
        {
            myNewTex.SetPixel(i, 1, Color.red);
        }

        myNewTex.Apply();
        OutMarcerLine.GetComponent<RawImage>().texture = myNewTex;
    }

    void OnApplicationQuit() // При закрытии игры, не забываем отключить скрипт генерации карту глубин
    {
        print("____End");
        process.Kill();
    }

    public void Update() // Этот метод запускается каждый кадр
    {
        MyWorkCoroutine();

        //if (Input.GetKey(KeyCode.Space)) // Для отладки
        //{
        //    MyWorkCoroutine();
        //}
    }

    bool isPrint = false;

    void MyWorkCoroutine()
    {
        string fileName = "CamTexture_01";

        if (isPrint) print("Запустили генерацию карты глубин" + System.DateTime.Now.ToString("HH:mm:ss:ms"));

        if (isPrint) print("Сохранили 2 картинки с камер" + System.DateTime.Now.ToString("HH:mm:ss:ms"));

        string l_way = ExportImageWay + "Left" + fileName;
        string r_way = ExportImageWay + "Right" + fileName;

        if (isPrint) print("Задали пути для сохранения картинок" + System.DateTime.Now.ToString("HH:mm:ss:ms"));

        try
        {
            // Вот тут может возникнуть ошибка, если система не даёт доступа к изменению этих файлов
            // Но мы её игнорируем
            SaveTextureToFileUtility.SaveRenderTextureToFile(LeftCam, l_way);
            SaveTextureToFileUtility.SaveRenderTextureToFile(RightCam, r_way);
        }
        catch (Exception ex)
        {
            // При ошибке ничего не делаем, просто продолжаем выполнение программы
        }


        if (isPrint) print("Сохранили 2 картинки с камер" + System.DateTime.Now.ToString("HH:mm:ss:ms"));

        try
        {
            LoadTexture();  // Вот тут можно добавить условие после выполнения скрипта
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("При загрузке изображения произошла ошибка: " + ex);
        }

        if (isPrint) print("Генерация карты глубин завершена" + System.DateTime.Now.ToString("HH:mm:ss:ms"));
    }

    void LoadTexture()
    {
        string imagePath = ImportDephMapWay; 
        
        // Загрузка изображения в экземпляр Texture2D
        Texture2D
        tex = new Texture2D(2, 2); // Создаем экземпляр Texture2D
        byte[] fileData = File.ReadAllBytes(imagePath); // Читаем данные изображения из файла
        tex.LoadImage(fileData); // Загружаем данные в Texture2D

        DephMapImage.GetComponent<RawImage>().texture = tex; // Выгружаем изображение карты глубин в одно из окон, в интерфейсе программы

        if (isPrint) print("Обновили текстуру " + System.DateTime.Now.ToString("HH:mm:ss:ms"));

        OnPixelCompression(tex); // Сжимаем текстуру карты глубины в вертикальные линии

        CheckObstacles(); // Проверяем наличие препятствий, используя сжатую карту глубин
    }

    public bool thereIsObstacle = false;    // В центральной зоне есть препятствие?
    public bool isEmergyStop = false;       // Препятствие в центральной зоне угрожающе близко?
    float emergyObstacle = 0.9f;

    // Значения, для определения, в какую сторонй следует поворачиваться
    public float leftNoiseValue;
    public float rightNoiseValue;

    public int whatTurn = 0; // Определяемся, в какую сторону будем поворачиваться

    void CheckObstacles() 
    {
        thereIsObstacle = false;    // Требуется замедление
        isEmergyStop = false;       // Требуется полная остановка

        leftNoiseValue = 0f;
        rightNoiseValue = 0f;

        // Прохожу по всем значениям в массиве, которые попадают под маркер центрального препятствия
        for (int i = (allCountRixels / 2) - marcerOffset; i < (allCountRixels / 2) + marcerOffset; i++)
        {
            if (colArray[i] > doorstep) thereIsObstacle = true;     // Если есть любое препятствие
            if (colArray[i] > emergyObstacle) isEmergyStop = true;  // Если есть угрожающе близкое препятствие
        }

        if (thereIsObstacle == true) // Если препятствие всё-же есть, рассчитываю, в какой стороне меньше коэффициент близости препятствия
        {
            float bufer = 0;

            for (int i = 0; i < (allCountRixels / 2) - marcerOffset; i++)
            {
                bufer += colArray[i];
            }

            leftNoiseValue = (bufer / 26);

            bufer = 0;

            for (int i = (allCountRixels / 2) + marcerOffset + 1; i < allCountRixels; i++)
            {
                bufer += colArray[i];
            }

            rightNoiseValue = (bufer / 25);
        }

        DodgingObstacles(); // Метод, в котором я, при необходимости, отдаю команды персонажу
    }

    int emStopCounterInit = 2;
    public int emStopCounter = 2;       // Делаю задержку для полной остановки, так как на карте глубины могут всплывать быстро исчезающие шумы

    void DodgingObstacles()
    {
        bool wall_p = false;
        bool arr_l = false;
        bool arr_r = false;
        bool em_st = false;

        if (thereIsObstacle == false)
        {
            PlayerController_01.PlayerControl_StandartSpeedMove(); // Если препятствий нет
            whatTurn = 0;
        }
        else // Если они есть
        {
            if (isEmergyStop == true) // Если нужна полная остановка
            {
                if (emStopCounter > 0)
                {
                    emStopCounter--;
                    //print("Недостаточно данных для полной остановки");
                }
                else
                {
                    PlayerController_01.PlayerControl_EmergyStop(); // Если это всё-таки не помехи, и остановка действительно требуется
                    //print("Полная остановка выполнена");
                    em_st = true;
                }
            }
            else
            {
                PlayerController_01.PlayerControl_Suspend(); // Если будет достаточно только замедления
                wall_p = true;
            }

            if (whatTurn == 0) // Если направление поворота ещё не выбрано
            {
                if (leftNoiseValue > rightNoiseValue)
                {
                    whatTurn = 1;
                }
                else
                {
                    whatTurn = 2;
                }
            }

            if (whatTurn == 1)
            {
                //print("Поворачиваемя направо");
                PlayerController_01.PlayerControl_TurnRight();
                arr_r = true;
            }
            else if (whatTurn == 2)
            {
                //print("Поворачиваемя налево");
                PlayerController_01.PlayerControl_TurnLeft();
                arr_l = true;
            }
        }

        if (isEmergyStop == false)
        {
            if (emStopCounter < emStopCounterInit) emStopCounter++;
        }

        speedText.text = "Speed = " + Math.Round(PlayerController_01.Speed, 1).ToString(); // Также округляю для лучшего результата

        // Включаю и выключаю картинки, в главном окне:
        Wall.enabled = wall_p;
        Arr_left.enabled = arr_l;
        Arr_right.enabled = arr_r;
        Em_Stop.enabled = em_st;
    }

    public float[] colArray = new float[allCountRixels]; // Прописан тут, для отладки   
    float doorstep = 0.45f; // Порог распознавания препятствий. Чем значение меньше - тем раньше препятствие появится на текстуре из линий

    Texture2D OnPixelCompression(Texture2D texture)
    {
        int currCountUse = 0;           // Число рядов, которые мы прошли. 10 рядов суммируются в одну ячейку в массиве
        int currIndOfMass = 0;          // Индекс в массиве        
        int countNoisyPixels = 20;      // Сколько шумных пикселей мы пропустим в блоке, перед распознаванием самого большого значения
        int noDetection = countNoisyPixels;   

        Array.Clear(colArray, 0, colArray.Length); // Сначала очищаем массив

        for (int w = 0; w < texture.width; w++)
        {
            for (int h = 0; h < texture.height; h++)
            {
                float currPixel = texture.GetPixel(w, h).r;

                if (noDetection > 0)
                {
                    if (currPixel > doorstep)
                    {
                        noDetection--;
                    }
                }
                else
                {
                    if (currPixel > colArray[currIndOfMass]) colArray[currIndOfMass] = currPixel;
                }
            }

            currCountUse++;

            if (currCountUse >= 10) // Если мы уже прошли 10 раядов, то переходим на заполнение следующей ячейки в массиве
            {
                currCountUse = 0;
                currIndOfMass++;
                noDetection = countNoisyPixels;
            }
        }

        Texture2D myTex = new Texture2D(58, 1); // Тут было (58, 32)

        for (int w1 = 0; w1 < 58; w1++)
        {
            Color newPixelColor = new Color(colArray[w1], colArray[w1], colArray[w1], 1f);
            myTex.SetPixel(w1, 0, newPixelColor);
        }

        myTex.Apply();

        OutLineTexture.GetComponent<RawImage>().texture = myTex;
        return myTex;
    }
}
