using System.Drawing;
using System.Diagnostics;
using TestGenerator;
using Inference;

#pragma warning disable CA1416


class program
{
    public static void Main(string[] args)
    {

        Console.Write("0 for user input, 1 for generated tests : ");
        int n = Convert.ToInt32(Console.ReadLine());

        if (n == 0)
        {
            Console.Write("Enter the Image path : ");
            string path = Console.ReadLine();
            Bitmap img = new Bitmap(path);
            Classifier.PrintResults(img);
        }
        else if (n == 1)
        {
            Console.Write("Enter the Number of tests to generate : ");
            int numTests = Convert.ToInt32(Console.ReadLine());

            Console.Write("Enter the Number of shapes to generate 1-200 : ");
            int numShapes = Convert.ToInt32(Console.ReadLine());
            
            for (int i = 1; i <= numTests; i++)
            {
                Bitmap img = Generator.GenerateTest(i, numShapes);
                Classifier.PrintResults(img, i);
            }
        }
    
        else
        {
            Console.Write("Wrong input");
        }

    }
}
