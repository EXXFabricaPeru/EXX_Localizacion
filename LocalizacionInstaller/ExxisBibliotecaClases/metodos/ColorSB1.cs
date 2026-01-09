namespace ExxisBibliotecaClases.metodos
{
    public class ColorSB1
    {
        public static int AMARILLO {
            get {
                return Parse(255, 255, 0);
            }
        }
        public static int GRIS
        {
            get
            {
                return Parse(180, 180, 180);
            }
        }
        public static int ROJO
        {
            get
            {
                return Parse(255, 0, 0);
            }
        }
        public static int VERDE
        {
            get
            {
                return Parse(0, 255, 0);
            }
        }
        public static int AZUL
        {
            get
            {
                return Parse(0, 0, 255);
            }
        }
        public static int NARANJA
        {
            get
            {
                return Parse(253, 165, 0);
            }
        }
        public static int NINGUNO
        {
            get
            {
                return -1;
            }
        }
        public static int Parse(int r, int g, int b){
            int clInt = r | (g << 8) | (b << 16);
            return clInt;
        }
    }
}
