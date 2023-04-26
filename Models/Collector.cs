namespace SolarABC.Models
{
    public enum TemperatureScale : ushort
    {
        Celsius = 0,
        Fahrenheit = 1
    }
    public class Collector
    {
        public double Width { get; set; }
        public double Length { get; set; }
        public double SolarIrradiation { get; set; }
        public double SpecificHeat { get; set; }
        public double FluidInTemp { get; set; }
        // public double RecieverTemp => getTR();
        public double AmbientTemp { get; set; }
        public double SkyTemp => (double)0.0552 * (Math.Pow(AmbientTemp, 1.5));
        public double AbsorberEmmitance { get; set; }
        public double CoverEmmitance { get; set; }
        public double CoverThermalConductivity { get; set; }
        public double WindMassFlow { get; set; }
        public double ApertureArea => (double)(Width * Length);
    }

    public class PTC : Collector
    {
        public double InnerRecieverDiameter { get; set; }
        public double OuterRecieverDiameter { get; set; }
        public double InnerCoverDiameter { get; set; }
        public double OuterCoverDiameter { get; set; }
        public double Mi { get; set; }
        public double WindConvectiveCoefficient => getHw();
        public double RecieverArea => (double)((OuterRecieverDiameter * Math.PI) * Length); //Ar = PI D L
        public double ConcentratingRatio => (double)(ApertureArea / RecieverArea); //C = Aa/Ar
        public double AvailableSolarIrradiation => (double)(ApertureArea * SolarIrradiation); //Qs = Aa * Gb

        public double getHw()
        {
            double airMi = (1.794 * Math.Pow(10, -5));
            double airDensity = 1.232, airThermalConductivity = 0.025, reynolds, prandtl, nusselt;
            reynolds = (4 * WindMassFlow) / (Math.PI * InnerRecieverDiameter * airMi);
            prandtl = airMi * SpecificHeat / airThermalConductivity;
            nusselt = reynolds > 0.1 && reynolds < 1000 ? laminarNusseltCorrelation(reynolds) : turbulentNusseltCorrelation(reynolds, prandtl);
            return (nusselt * airThermalConductivity) / OuterCoverDiameter;
        }

        public double laminarNusseltCorrelation(double reynolds)
        {
            return 0.40 + (0.54 * Math.Pow(reynolds, 0.52));
        }

        public double turbulentNusseltCorrelation(double reynolds, double prandtl)
        {
            return 0.023 * Math.Pow(reynolds, 0.8) * Math.Pow(prandtl, 0.8);
        }

        public PTC(double width, double length, double Gb, double Cp, double Mi, double Tam, double Er, double Ec, double Kc, double mDotWind, double Dri, double Dro, double Dci, double Dco)
        {
            this.Width = width;
            this.Length = length;
            this.SolarIrradiation = Gb;
            this.AmbientTemp = Tam;
            this.AbsorberEmmitance = Er;
            this.CoverEmmitance = Ec;
            this.CoverThermalConductivity = Kc;
            this.WindMassFlow = mDotWind;
            this.InnerRecieverDiameter = Dri;
            this.OuterRecieverDiameter = Dro;
            this.InnerCoverDiameter = Dci;
            this.OuterCoverDiameter = Dco;
        }

        public double getKelvinTemp(double temp, TemperatureScale tScale)
        {
            switch (tScale)
            {
                case TemperatureScale.Celsius:
                    return temp + 273.15;
                case TemperatureScale.Fahrenheit:
                    return Convert.ToDouble(((5 / 9) * temp - 32) + 273.15);
                default:
                    return temp;
            }
        }
    }
}