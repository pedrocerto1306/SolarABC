namespace SolarABC.Models
{
    public class Statistics
    {
        public double TotalLossCoefficient { get; set; }
        public double Qu { get; set; }
        public double ThermalEfficiency { get; set; }
        public double Tout { get; set; }

        public Statistics(PTC coll, EntryData et)
        {
            double[] results = iteractiveProcess(coll);
            this.TotalLossCoefficient = results[0];
            this.Qu = (coll.AvailableSolarIrradiation * 0.80) - results[1];
            this.ThermalEfficiency = (this.Qu) / coll.AvailableSolarIrradiation;
            this.Tout = (this.Qu / (et.m * et.Cp)) + coll.FluidInTemp;
        }

        private double[] iteractiveProcess(PTC c)
        {
            int maxIterations = 500;
            double qlossError = 0.001; //Acceptable variance between Qloss values (works as stop condition in the algorithm)
            double sigma = 5.67 * Math.Pow(10, -8); //Stefan-Boltzmann Constant
            double coverTemp = (new Random().NextDouble() * c.AmbientTemp) + c.AmbientTemp;
            double qLoss1 = (Math.PI * c.OuterCoverDiameter * c.Length * c.WindConvectiveCoefficient *
            (coverTemp - c.AmbientTemp)) + (c.CoverEmmitance * Math.PI * c.OuterCoverDiameter * c.Length * sigma
            * (Math.Pow(coverTemp, 4) * Math.Pow(c.SkyTemp, 4)));
            double innerCoverTemp = coverTemp + ((qLoss1 * (Math.Log(c.OuterCoverDiameter / c.InnerCoverDiameter)))
                                    / (2 * Math.PI * c.CoverThermalConductivity * c.Length));

            //Calcular temperatura do receptor
            double recieverTemp = Math.Pow(Math.Abs(((1 / c.AbsorberEmmitance + ((1 - c.CoverEmmitance / c.CoverEmmitance) * (c.OuterRecieverDiameter / c.InnerCoverDiameter))) * qLoss1 / (Math.PI * c.OuterRecieverDiameter * c.Length * sigma)) + Math.Pow(coverTemp, 4)), 1 / 4);

            double qLoss2 = (Math.PI * c.OuterRecieverDiameter * c.Length * sigma *
            (Math.Pow(recieverTemp, 4) - Math.Pow(innerCoverTemp, 4))) /
            ((1 / c.AbsorberEmmitance) + (((1 - c.CoverEmmitance) / c.CoverEmmitance) *
            (c.OuterRecieverDiameter / c.InnerCoverDiameter)));

            while (Math.Abs(qLoss2 - qLoss1) > qlossError && maxIterations > 0)
            {
                //In case the first Qloss is less than the second one, the estimated is too low, so it value is increased,
                // otherwise it's decreased
                coverTemp = qLoss1 - qLoss2 < 0 ? coverTemp + (coverTemp * 0.02) : coverTemp - (coverTemp * 0.02);
                qLoss1 = (Math.PI * c.OuterCoverDiameter * c.Length * c.WindConvectiveCoefficient * (coverTemp - c.AmbientTemp))
                + (c.CoverEmmitance * Math.PI * c.OuterCoverDiameter
                * c.Length * sigma * (Math.Pow(coverTemp, 4) * Math.Pow(c.SkyTemp, 4)));
                innerCoverTemp = coverTemp + ((qLoss1 * (Math.Log(c.OuterCoverDiameter / c.InnerCoverDiameter)))
                / (2 * Math.PI * c.CoverThermalConductivity * c.Length));
                recieverTemp = Math.Pow(Math.Abs(((1 / c.AbsorberEmmitance + ((1 - c.CoverEmmitance / c.CoverEmmitance) * (c.OuterRecieverDiameter / c.InnerCoverDiameter))) * qLoss1 / (Math.PI * c.OuterRecieverDiameter * c.Length * sigma)) + Math.Pow(coverTemp, 4)), 1 / 4);
                qLoss2 = (Math.PI * c.OuterRecieverDiameter * c.Length * sigma * (Math.Pow(recieverTemp, 4)
                - Math.Pow(innerCoverTemp, 4)));
                maxIterations--;
            }

            double[] results = { (qLoss2 / (Math.PI * c.OuterRecieverDiameter * c.Length * (recieverTemp - c.AmbientTemp))), qLoss1 };

            return results;
        }
    }
}