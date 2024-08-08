using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
class Node
{
    public Rectangle bounds;
    public int depth;
    public Color averageColor;
    public double detail;
    public bool isLeaf;
    public List<Node> children;

    public Node(Bitmap image, Rectangle bounds, int depth)
    {
        this.bounds = bounds;
        this.depth = depth;
        isLeaf = false;
        children = new List<Node>();

        using (Bitmap croppedImage = image.Clone(bounds,image.PixelFormat)) 
        {
            averageColor = CalculateAverageColor(croppedImage);
            detail = CalculateDetail(croppedImage);
        }
    }
    private Color CalculateAverageColor(Bitmap image)
    {
        Color pixelColor = Color.Black;

        int sumR = 0;
        int sumG = 0;
        int sumB = 0;

        int R, G, B;

        // petla przez wszystkie piksele
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                pixelColor = image.GetPixel(i, j);

                R = pixelColor.R;
                sumR += R;
                G = pixelColor.G;  
                sumG += G;
                B = pixelColor.B;
                sumB += B;
            }
        }

        int pixelsAmount = image.Width * image.Height;

        int averageR = sumR/ pixelsAmount;
        int averageG = sumG/ pixelsAmount;
        int averageB = sumB/ pixelsAmount;

        // zamiana srednich wartosci R G B na jeden kolor
        Color AverageColor = Color.FromArgb(averageR, averageG, averageB);

        return AverageColor;
    }

    private double CalculateDetail(Bitmap image)
    {
        double variance = 0;

        Color averageColor = CalculateAverageColor(image);
        Color pixelColor;

        double tempR = 0;
        double tempG = 0;
        double tempB = 0;
        double sum = 0;

        for (int i = 0; i < image.Width; i++) 
        {
            for (int j = 0; j < image.Height; j++)
            {
                // licze wariancje kolorow

                pixelColor = (image.GetPixel(i, j));

                tempR = pixelColor.R - averageColor.R;
                tempG = pixelColor.G - averageColor.G;
                tempB = pixelColor.B - averageColor.B;

                sum += Math.Pow(tempR, 2) + Math.Pow(tempG, 2) + Math.Pow(tempB, 2);
            }
        }

        int pixelsAmount = image.Width * image.Height;
        variance = sum / pixelsAmount;

        double result = Math.Sqrt(variance);

        return result;
    }

    public void Split(Bitmap image, int maxDepth, double detailThreshold)
    {
        if (depth >= maxDepth || detail <= detailThreshold)
        {
            isLeaf = true;
            return;
        }

        // okreslam wspolrzedne srodka obrazu
        int middleX = bounds.X + bounds.Width / 2;
        int middleY = bounds.Y + bounds.Height / 2;

        Rectangle tl = new Rectangle(bounds.X, bounds.Y, middleX - bounds.X, middleY - bounds.Y);
        Rectangle tr = new Rectangle(middleX, bounds.Y, bounds.Right - middleX, middleY - bounds.Y);
        Rectangle bl = new Rectangle(bounds.X, middleY, middleX - bounds.X, bounds.Bottom - middleY);
        Rectangle br = new Rectangle(middleX, middleY, bounds.Right - middleX, bounds.Bottom - middleY);

        // szerokosc i wysokosc nie moga byc ujemne
        if (tl.Width > 0 && tl.Height > 0 )
        {
            children.Add(new Node(image, tl, depth + 1));
        }
        if (tr.Width > 0 && tr.Height > 0)
        {
            children.Add(new Node(image, tr, depth + 1));
        }
        if (bl.Width > 0 && bl.Height > 0)
        {
            children.Add(new Node(image, bl, depth + 1));
        }
        if (br.Width > 0 && br.Height > 0)
        {
            children.Add(new Node(image, br, depth + 1));
        }

        foreach (var child in children)
        {
            child.Split(image, maxDepth, detailThreshold);
        }
    }
}

class QuadTree
{
    private Node root;
    private int width;
    private int height;
    public QuadTree(Bitmap image, int maxDepth, double detailThreshold)
    {
        width = image.Width;
        height = image.Height;

        // korzen jest calym obrazem
        root = new Node(image, new Rectangle(0, 0, width, height), 0);

        root.Split(image, maxDepth, detailThreshold);
    }

    public Bitmap Createimage()
    {
        Bitmap result = new Bitmap(width, height);

        using(Graphics graphics = Graphics.FromImage(result)) 
        {
            graphics.Clear(Color.Black);
            DrawNode(graphics, root);
        }
        return result;
    }

    public void DrawNode(Graphics graphics, Node node)
    {
        if (node.isLeaf == true)
        {
            // rysuje kontury podzialu
            using (SolidBrush brush = new SolidBrush(node.averageColor)) 
            {
                graphics.FillRectangle(brush, node.bounds);
                using(Pen pen = new Pen(Color.Black))
                {
                    graphics.DrawRectangle(pen, node.bounds);
                }
            }
        }
        else
        {
            foreach(var child in node.children)
            {
                DrawNode(graphics, child);
            }
        }
    }
}

class Program
{
    static void Main()
    {
        string imagePath = "C:\\Users\\Natalia\\source\\repos\\t\\m.jpg";
        Bitmap image = new Bitmap(imagePath);

        int maxDepth = 6;
        double detailThreshold = 12.0;
            
        QuadTree quadtree = new QuadTree(image, maxDepth, detailThreshold);
        Bitmap outputImage = quadtree.Createimage();

        outputImage.Save("C:\\Users\\Natalia\\source\\repos\\t\\result.jpg", ImageFormat.Jpeg);

        return;
    }
}