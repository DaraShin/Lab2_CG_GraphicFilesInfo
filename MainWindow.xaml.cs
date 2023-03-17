using System.IO;
using System.Windows;
using System.Drawing.Imaging;
using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using MetadataExtractor;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Lab2_CG_GraphicFiles2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> fileExtensions = new List<string>() { ".jpg", ".gif", ".tiff", ".tif", ".bmp", ".png", ".pcx" };
        private Dictionary<int, string> compressionTypes = new Dictionary<int, string>()
        {
            { 1, "Uncompressed" },
            { 2 , "CCITT 1D" },
            { 3 , "T4 / Group 3 Fax" },
            { 4 , "T6 / Group 4 Fax" },
            { 5 , "LZW" },
            { 6 , "JPEG(old - style)" },
            { 7 , "JPEG" },
            { 8 , "Adobe Deflate" },
            { 9 , "JBIG B & W" },
            { 10 , "JBIG Color" },
            { 99 , "JPEG" },
            { 262 , "Kodak 262" },
            { 32766 , "Next" },
            { 32767 , "Sony ARW Compressed" },
            { 32769 , "Packed RAW" },
            { 32770 , "Samsung SRW Compressed" },
            { 32771 , "CCIRLEW" },
            { 32772 , "Samsung SRW Compressed 2" },
            { 32773 , "PackBits" },
            { 32809 , "Thunderscan" },
            { 32867 , "Kodak KDC Compressed" },
            { 32895 , "IT8CTPAD" },
            { 32896 , "IT8LW" },
            { 32897 , "IT8MP" },
            { 32898 , "IT8BL" },
            { 32908 , "PixarFilm" },
            { 32909 , "PixarLog" },
            { 32946 , "Deflate" },
            { 32947 , "DCS" },
            {33003 , "Aperio JPEG 2000 YCbCr" },
            {33005 , "Aperio JPEG 2000 RGB" },
            {34661 , "JBIG" },
            {34676 , "SGILog" },
            {34677 , "SGILog24" },
            {34712 , "JPEG 2000" },
            {34713 , "Nikon NEF Compressed" },
            {34715 , "JBIG2 TIFF FX" },
            {34718 , "Microsoft Document Imaging(MDI) Binary Level Codec" },
            {34719 , "Microsoft Document Imaging(MDI) Progressive Transform Codec" },
            {34720 , "Microsoft Document Imaging(MDI) Vector" },
            {34887 , "ESRI Lerc" },
            {34892 , "Lossy JPEG" },
            {34925 , "LZMA2" },
            {34926 , "Zstd" },
            {34927 , "WebP" },
            {34933 , "PNG" },
            {34934 , "JPEG XR" },
            {65000 , "Kodak DCR Compressed" },
            {65535 , "Pentax PEF Compressed" },
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void onChooseFilesBtnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
           
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filesGrid.Items.Clear();
                FileInfo[] filesInfo = new FileInfo[openFileDialog.FileNames.Length];
                for(int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    filesInfo[i] = new FileInfo(openFileDialog.FileNames[i]);
                }
                if(filesInfo.Length > 0)
                {
                    folderTextBox.Text = filesInfo[0].Directory.FullName;
                }
                
                showFilesInfo(filesInfo);
            }
        }

        private void onChooseFolderBtnClick(object sender, RoutedEventArgs e)
        {
            
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filesGrid.Items.Clear();
                string folderPath = folderBrowserDialog.SelectedPath;
                folderTextBox.Text = folderPath;

                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                FileInfo[] filesInFolderInfo = directoryInfo.GetFiles()
                    .Where(fileInfo => fileExtensions.Contains(System.IO.Path.GetExtension(fileInfo.FullName).ToLower()))
                    .ToArray();

                showFilesInfo(filesInFolderInfo);
            }
        }

        private async void showFilesInfo(FileInfo[] filesToShow)
        {
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8
            };

            await Task.Run(() =>
            {
                Parallel.For(0, filesToShow.Length, options, (i) =>
                {
                    FilesGridRow row = new FilesGridRow();
                    FileInfo fileInfo = filesToShow[i];

                    if (fileInfo.Extension != ".pcx")
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromFile(fileInfo.FullName);

                        row.fileName = fileInfo.Name;
                        row.size = img.Width + "x" + img.Height;
                        row.resolution = Math.Round(img.HorizontalResolution) + "x" + Math.Round(img.VerticalResolution);

                        try
                        {
                            row.colorDepth = GetColorDepth(img.PixelFormat).ToString();
                        }
                        catch (ArgumentOutOfRangeException exc)
                        {
                            row.colorDepth = "Unknown";
                        }
                        row.compression = getCompression(img);
                        img.Dispose();

                        Dispatcher.Invoke(() =>
                        {

                            filesGrid.Items.Add(row);
                        });
                    }
                    else
                    {
                        row = readPcxMetadata(fileInfo.FullName);
                        Dispatcher.Invoke(() =>
                        {
                            filesGrid.Items.Add(row);
                        });
                    }
                });
            });
        }

        private FilesGridRow readPcxMetadata(string filePath)
        {
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            FilesGridRow row = new FilesGridRow();

            // save metadata to dictionary
            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    //Trace.WriteLine($"{directory.Name} - {tag.Name}, {tag.Description}");
                    metadata.Add($"{directory.Name} - {tag.Name}", $"{tag.Description}");
                }
            }

            row.fileName = metadata["File - File Name"];
            row.size = (Convert.ToInt32(metadata["PCX - X Max"]) - Convert.ToInt32(metadata["PCX - X Min"]))
                + "x"
                + (Convert.ToInt32(metadata["PCX - Y Max"]) - Convert.ToInt32(metadata["PCX - Y Min"]));
            row.resolution = metadata["PCX - Horizontal DPI"] + "x" + metadata["PCX - Vertical DPI"];
            row.colorDepth = metadata["PCX - Color Planes"];

            byte[] bytes = new byte[3];
            using (var reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                reader.Read(bytes, 0, 3);
            }
            var isCompressed = bytes[2] == 1;
            row.compression = isCompressed ? "RLE" : "Uncompressed";
            return row;
        }


        private string getCompression(System.Drawing.Image img)
        {
            try
            {
                PropertyItem propertyItem = img.GetPropertyItem(0x0103);
                Int16 compressionCode = BitConverter.ToInt16(propertyItem.Value, 0);
                return compressionTypes[compressionCode];
            }
            catch (ArgumentException exc)
            {
                return "Can't be defined";
            }
            catch (KeyNotFoundException exc)
            {
                return "Unknown type";
            }
        }

        private int GetColorDepth(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format4bppIndexed:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 8;
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                    return 16;
                case PixelFormat.Format24bppRgb:
                    return 24;
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return 32;
                case PixelFormat.Format48bppRgb:
                    return 48;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }
}
