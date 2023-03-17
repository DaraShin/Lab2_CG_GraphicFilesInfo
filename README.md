# Автор
Шинкевич Дарья, 13 группа
# О приложении
Приложение для чтения информации из графических файлов.  
Приложение отображает следующую информацию о файле:
* имя файла;
* размер изображения (в пикселях);
* разрешение (dot/inch);
* глубину цвета;
* сжатие.  
Приложение обрабатывает следующие форматы файлов: jpg, gif, tif, tiff, bmp, png, pcx.
![image](https://user-images.githubusercontent.com/78850433/225841190-d3b6ebe6-3023-498a-a665-311b280bcffa.png)
# Установка и запуск
1. Склонировать репозиторий:  
<code>git clone git@github.com:DaraShin/Lab2_CG_GraphicFilesInfo.git</code>
2. Перейти в папку app_exe:  
<code>cd Lab2_CG_GraphicFilesInfo\app_exe</code>
3. Запустить файл Lab2_CG_GraphicFiles2.exe на выполнение:
<code>Lab2_CG_GraphicFiles2.exe</code>
# Сборка
1. Склонировать репозиторий:  
<code>git clone git@github.com:DaraShin/Lab2_CG_GraphicFilesInfo.git</code>
2. Перейти в папке:  
<code>cd Lab2_CG_GraphicFilesInfo\src</code>
3. Выполнить сборку с помощью dotnet:  
<code>dotnet build  /p:Configuration=Release /p:Platform=AnyCPU</code>
4. Исполняемый .exe файл находится в папке:  
<code>.\bin\Release\net6.0-windows\Lab2_CG_GraphicFiles2.exe</code>
# Команда dotnet
Для сборки приложения должна быть установлена платфома .NET.  
Ссылка для установки: [https://dotnet.microsoft.com/en-us/download/dotnet?cid=getdotnetcorecli](https://dotnet.microsoft.com/en-us/download/dotnet?cid=getdotnetcorecli)  
Файл dotnet.exe можно найти по следующему пути: <code>С:\Program Files\dotnet\dotnet.exe</code>
# Используемые технологии 
* WPF (Windows Presentation Foundation)
* .NET 6.0
# Документация
Для получения информации о файлах графических изображений использован класс <code>Image</code> из библиотеки <code>System.Drawing</code>. С его помощью получена информация о размере и расширении изображения, o глубине цвета (свойство <code>PixelFormat</code>):

```System.Drawing.Image img = System.Drawing.Image.FromFile(fileInfo.FullName);
row.fileName = fileInfo.Name;
row.size = img.Width + "x" + img.Height;
row.resolution = Math.Round(img.HorizontalResolution) + "x" + Math.Round(img.VerticalResolution);
```
Получение информации о сжатии:  
```
 PropertyItem propertyItem = img.GetPropertyItem(0x0103);
Int16 compressionCode = BitConverter.ToInt16(propertyItem.Value, 0);
return compressionTypes[compressionCode];
```
 
Информация о pcx-файле получена из метаданных с помощью класса <code>ImageMetadataReader</code> из библиотеки <code>MetadataExtractor</code>.
Для получения информации о сжатии pcx-файла использовано байтовое представление файла.

Для ускорения обработки большого количества файлов использовано многопточное выполнение.
