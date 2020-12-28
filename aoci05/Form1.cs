using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.OCR; //модуль оптического распознавания символов

namespace aoci05
{
    public partial class Form1 : Form
    {
        Image<Bgr, byte> sourceImage; //глобальная переменная
        List<Rectangle> rois = new List<Rectangle>();
        List<Rectangle> words = new List<Rectangle>();

        Tesseract ocrE = new Tesseract();
        Tesseract ocrR = new Tesseract();

        public Form1()
        {
            InitializeComponent();
        }

        private Image<Bgr,byte> open()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                sourceImage = new Image<Bgr, byte>(fileName);

                imageBox1.Image = sourceImage;//.Resize(640, 480, Inter.Linear);
            }
            return sourceImage;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            open();

            ocrE = new Tesseract("D:\\", "eng", OcrEngineMode.TesseractLstmCombined);
            ocrR = new Tesseract("D:\\", "rus", OcrEngineMode.TesseractLstmCombined);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public Image<Gray, byte> rasshir(Image<Bgr, byte> sourceImage, int m)
        {
            var gray = sourceImage.Convert<Gray, byte>();
            
            Image<Gray, byte> grayIm;
            grayIm = gray.ThresholdBinaryInv(new Gray(m), new Gray(255));
            //var dilatedImage = gray.Dilate(5);
            return grayIm;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var gray = sourceImage.Convert<Gray, byte>();
            //gray._ThresholdBinaryInv(new Gray(128), new Gray(255));

            //thresh._Dilate(5);
            imageBox2.Image = rasshir(sourceImage, trackBar2.Value);
        }

        public Image<Gray, byte> dilated(Image<Bgr, byte> sourceImage, int a)
        {
            var binarizedImage = rasshir(sourceImage, a);

            var dilatedImage = binarizedImage.Dilate(a);

            return dilatedImage;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //var gray = sourceImage.Convert<Gray, byte>();
            //gray._ThresholdBinaryInv(new Gray(128), new Gray(255));

            ////thresh._Dilate(5);
            //imageBox2.Image = gray;

            //var dilatedImage = gray.Dilate(5);

            imageBox2.Image = dilated(sourceImage, trackBar1.Value);

        }

        public Image<Bgr, byte> fRoi(Image<Bgr, byte> sourceImage, List<Rectangle> rois, int m)
        {
            //var dilatedImage = gray.Dilate(5);
            var dilatedImage = dilated(sourceImage, m);

            //VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(dilatedImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            var output = sourceImage.Copy();
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i], false) > m * 10) //игнорирование маленьких контуров
                {
                    Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                    rois.Add(rect);
                    output.Draw(rect, new Bgr(Color.Blue), 1);

                }
            }

            return output;
        }


        private void button4_Click(object sender, EventArgs e)
        {
            string text = "";
            words.Clear();
            listBox1.Items.Clear();
            imageBox2.Image = fRoi(sourceImage, words, trackBar2.Value);
            for (int i = 0; i < words.Count; i++)
            {
                sourceImage.ROI = words[i];
                var roiCopy = sourceImage.Copy();
                sourceImage.ROI = Rectangle.Empty;

                ocrE.SetImage(roiCopy);
                ocrE.Recognize();
                text = ocrE.GetUTF8Text();
                listBox1.Items.Add(text);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            sourceImage.ROI = words[listBox1.SelectedIndex];
            var roiCopy = sourceImage.Copy();
            sourceImage.ROI = Rectangle.Empty;
            imageBox2.Image = roiCopy;
            label1.Text = Convert.ToString(listBox1.SelectedItem);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            words.Clear();
            listBox2.Items.Clear();
            imageBox2.Image = fRoi(sourceImage, words, trackBar1.Value);
            for (int i = 0; i < words.Count; i++)
            {
                sourceImage.ROI = words[i];
                var roiCopy = sourceImage.Copy();
                sourceImage.ROI = Rectangle.Empty;

                ocrR.SetImage(roiCopy);
                ocrR.Recognize();
                string text = ocrR.GetUTF8Text();
                listBox2.Items.Add(text);
            }

            //var gray = sourceImage.Convert<Gray, byte>();
            //gray._ThresholdBinaryInv(new Gray(128), new Gray(255));

            ////thresh._Dilate(5);
            ////imageBox2.Image = gray;

            //var dilatedImage = gray.Dilate(5);

            ////VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            //var contours = new VectorOfVectorOfPoint();
            //CvInvoke.FindContours(dilatedImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            //var output = sourceImage.Copy();
            //for (int i = 0; i < contours.Size; i++)
            //{
            //    if (CvInvoke.ContourArea(contours[i], false) > 50) //игнорирование маленьких контуров
            //    {
            //        Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
            //        rois.Add(rect);
            //        output.Draw(rect, new Bgr(Color.Blue), 1);



            //        sourceImage.ROI = rect; // rois[0];
            //        var roiCopy = sourceImage.Copy();
            //        sourceImage.ROI = Rectangle.Empty;

            //        ocrR.SetImage(roiCopy); //фрагмент изображения, содержащий текст
            //        ocrR.Recognize(); //распознание текста
            //        //Tesseract.Character[] words = ocr.GetCharacters(); //получение найденных символов
            //        string text = ocrR.GetUTF8Text();

            //        //MessageBox.Show(text);
            //        listBox1.Items.Add(text);

            //        imageBox2.Image = output;
            //        //imageBox3.Image = roiCopy;
            //    }
            //}
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            sourceImage.ROI = words[listBox2.SelectedIndex];
            var roiCopy = sourceImage.Copy();
            sourceImage.ROI = Rectangle.Empty;
            imageBox2.Image = roiCopy;
            label2.Text = Convert.ToString(listBox2.SelectedItem);
        }

        public Image<Bgr, byte> DetectFace(Image<Bgr, byte> sourceImage, Image<Bgr, byte> second)
        {

            using (CascadeClassifier cascadeClassifier = new CascadeClassifier("D:\\haarcascade_frontalface_default.xml"))
            {
                var grayImage = sourceImage.Convert<Gray, byte>();
                // обнаружение лиц
                Rectangle[] facesDetected = cascadeClassifier.DetectMultiScale(grayImage, 1.1, 10,
                new Size(20, 20));
                // создание копии исходного изображения
                var copy = sourceImage.Copy();



                foreach (Rectangle face in facesDetected)
                {

                    var temp = second.Copy().Resize(face.Width, face.Height, Inter.Nearest);
                    second.Resize(face.Width, face.Height, Inter.Nearest);
                    copy.ROI = face;
                    // copy.Draw(face, new Bgr(Color.Yellow), 2);


                    CvInvoke.cvCopy(temp, copy, new IntPtr()); // temp - картинка для наложение copy - куда накладываем new IntPtr - заглушка ебаная
                }
                copy.ROI = Rectangle.Empty;
                return copy;
            }
        }
        public Image<Bgr, byte> DetectFaceWithoutMask(Image<Bgr, byte> sourceImage)
        {

            using (CascadeClassifier cascadeClassifier = new CascadeClassifier("D:\\haarcascade_frontalface_default.xml"))
            {
                var grayImage = sourceImage.Convert<Gray, byte>();
                // обнаружение лиц
                Rectangle[] facesDetected = cascadeClassifier.DetectMultiScale(grayImage, 1.1, 10,
                new Size(20, 20));
                // создание копии исходного изображения
                var copy = sourceImage.Copy();



                foreach (Rectangle face in facesDetected)
                {

                    // var temp = second.Copy().Resize(face.Width, face.Height, Inter.Nearest);
                    // second.Resize(face.Width, face.Height, Inter.Nearest);
                    //copy.ROI = face;
                    copy.Draw(face, new Bgr(Color.Yellow), 2);


                    //CvInvoke.cvCopy(temp, copy, new IntPtr()); // temp - картинка для наложение copy - куда накладываем new IntPtr - заглушка ебаная
                }
                //copy.ROI = Rectangle.Empty;
                return copy;
            }


        }
        private void button7_Click(object sender, EventArgs e)
        {
            imageBox2.Image = DetectFaceWithoutMask(sourceImage);
        }

        private VideoCapture capture;
        Image<Bgr, byte> temp;
        private void ProcessFrame_web(object sender, EventArgs e)
        {
            var frame = new Mat();
            capture.Retrieve(frame);
            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
            imageBox1.Image = image; //вывод кадра в нужном окне
            sourceImage = image;
            imageBox2.Image = DetectFace(sourceImage, temp);
        }
        private void button8_Click(object sender, EventArgs e)
        {
            temp = open();
            capture = new VideoCapture();
            capture.ImageGrabbed += ProcessFrame_web;
            capture.Start();
        }

        public string openV()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                return fileName;
            }
            return null;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            ocrE = new Tesseract("D:\\", "eng", OcrEngineMode.TesseractLstmCombined);
            ocrR = new Tesseract("D:\\", "rus", OcrEngineMode.TesseractLstmCombined);
            capture = new VideoCapture(openV());
            timer1.Enabled = true;
            capture.ImageGrabbed += timer1_Tick;
            capture.Start();
        }
        private int vframe = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            vframe++;
            if (vframe >= capture.GetCaptureProperty(CapProp.FrameCount))
            {
                timer1.Enabled = false;
                capture.ImageGrabbed -= timer1_Tick;

            }
            else
            {
                var frame = new Mat();
                capture.Retrieve(frame);

                Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
                imageBox1.Image = image;
                sourceImage = image;

                int tr1 = 0;
                int tr2 = 0;

                string text = "";

                words.Clear();
                Action action_clean = () =>

                {
                    listBox2.Items.Clear();
                    tr1 = trackBar1.Value;
                    tr2 = trackBar2.Value;

                };

                Action action_add = () =>
                {
                    listBox2.Items.Add(text);
                };
                Invoke(action_clean);

                imageBox2.Image = fRoi(sourceImage, words, tr1); 
                for (int i = 0; i < words.Count; i++)
                {
                    sourceImage.ROI = words[i];
                    var roiCopy = sourceImage.Copy();
                    sourceImage.ROI = Rectangle.Empty;

                    ocrE.SetImage(roiCopy);
                    ocrE.Recognize();
                    text = ocrE.GetUTF8Text();
                    Invoke(action_add);
                }

            }
        }
    }
}
