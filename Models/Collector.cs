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
        public double RecieverTemp { get; set; }
        public double AmbientTemp { get; set; }
        public double SkyTemp { get; set; }
        public double AbsorberEmmitance { get; set; }
        public double CoverEmmitance { get; set; }
        public double CoverThermalConductivity { get; set; }
        public double HeatTransferCoefficientInsideTube { get; set; }
        public double WindVelocity { get; set; }
        public double ApertureArea => (double)(Width * Length);
    }

    public class PTC : Collector
    {
        public double InnerRecieverDiameter { get; set; }
        public double OuterRecieverDiameter { get; set; }
        public double InnerCoverDiameter { get; set; }
        public double OuterCoverDiameter { get; set; }
        public double WindConvectiveCoefficient
        {
            get { return getHw(); }
        }
        public double RecieverArea => (double)((OuterRecieverDiameter * Math.PI) * Length); //Ar = PI D L
        public double ConcentratingRatio => (double)(ApertureArea / RecieverArea); //C = Aa/Ar
        public double AvailableSolarIrradiation => (double)(ApertureArea * SolarIrradiation); //Qs = Aa * Gb

        public double getHw()
        {
            double airDensity = 1.232, airDynamicViscosity = (1.794 * Math.Pow(10, -5)), airThermalConductivity = 0.025, reynolds, nusselt;
            reynolds = (airDensity * WindVelocity * OuterCoverDiameter) / airDynamicViscosity;
            nusselt = reynolds > 0.1 && reynolds < 1000 ? laminarNusseltCorrelation(reynolds) : turbulentNusseltCorrelation(reynolds);
            return (nusselt * airThermalConductivity) / OuterCoverDiameter;
        }

        public double laminarNusseltCorrelation(double reynolds)
        {
            return 0.40 + (0.54 * Math.Pow(reynolds, 0.52));
        }

        public double turbulentNusseltCorrelation(double reynolds)
        {
            return 0.30 * Math.Pow(reynolds, 0.6);
        }
    }
}