using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Policy;

namespace DemoIntroAsync
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;

            var directorioActual = AppDomain.CurrentDomain.BaseDirectory;

            var destinoBaseSecuencial = Path.Combine(directorioActual, @"Imagenes\resultado-secuencial");

            var destinoBaseParalelo = Path.Combine(directorioActual, @"Imagenes\resultado-paralelo");

            PrepararEjecucion(destinoBaseParalelo,
            destinoBaseSecuencial);

            Console.WriteLine("Inicio");
            List<Imagen> imagenes = ObtenerImagenes();

            //Parte Secuencial 
            var sw = new Stopwatch();

            sw.Start();

            foreach (var imagen in imagenes)
            {
                await ProcesarImagen(destinoBaseSecuencial, imagen);
            }

            Console.WriteLine("Secuencial - duración en segundos: {0}", sw.ElapsedMilliseconds / 1000.0);

            sw.Reset();

            sw.Start();

            var tareasEnumerable = imagenes.Select(async imagen =>
            {
                await ProcesarImagen(destinoBaseParalelo, imagen);
            });

            await Task.WhenAll(tareasEnumerable);

            Console.WriteLine("Paralelo - duración en segundos: {0}", sw.ElapsedMilliseconds / 1000.0);

            sw.Stop();

            pictureBox1.Visible = false;
        }


        private static List<Imagen> ObtenerImagenes()
        {
            var imagenes = new List<Imagen>();

            for (int i = 0; i < 7; i++)
            {

                imagenes.Add(new Imagen()
                {
                    Nombre = $"Camaro SS {i}.jpg",
                    URL = "https://i.gaw.to/content/photos/37/61/376185_2020_Chevrolet_Camaro.jpg"
                });

                imagenes.Add(new Imagen()
                {
                    Nombre = $"Porshe 911 Gt {i}.jpg",
                    URL = "https://www.classicdriver.com/sites/default/files/cars_images/img_8365_0.jpg"
                });


                imagenes.Add(new Imagen()
                {
                    Nombre = $"Mustang Gt {i}.jpg",
                    URL = "https://sportscarhangar.com.au/uploads/images/vehicles/_1200x630_crop_center-center_82_none/mustang-black-main.jpg?mtime=1602565578"
                });                
            }
            return imagenes;
        }





        private void BorrarArchivos(string directorio)
        {
            var archivos = Directory.EnumerateFiles(directorio);

            foreach (var archivo in archivos)
            {
                File.Delete(archivo);
            }

        }


        private void PrepararEjecucion(string destinoBaseParalelo, string destinoBaseSecuencial)

        {
            if (!Directory.Exists(destinoBaseParalelo))
            {
                Directory.CreateDirectory(destinoBaseParalelo);
            }

            if (!Directory.Exists(destinoBaseSecuencial))
            {
                Directory.CreateDirectory(destinoBaseSecuencial);
            }

            BorrarArchivos(destinoBaseSecuencial);
            BorrarArchivos(destinoBaseParalelo);

        }

        HttpClient httpClient = new HttpClient();


        private async Task ProcesarImagen(string directorio, Imagen imagen)

        {
            var respuesta = await httpClient.GetAsync(imagen.URL);

            var contenido = await respuesta.Content.ReadAsByteArrayAsync();

            Bitmap bitmap;

            using (var ms = new MemoryStream(contenido))
            {
                bitmap = new Bitmap(ms);

                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                var destino = Path.Combine(directorio, imagen.Nombre);
                bitmap.Save(destino);
            }

        }
    }
}

