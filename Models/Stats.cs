
namespace SolarABC.Models
{
    public class Stats
    {
        public double TotalLossCoefficient { get; set; }     //Ul
        public double EfficiencyFactor { get; set; }         //F'
        public double FlowFactor { get; set; }               //F"
        public double HeatRemovalFactor { get; set; }        //Fr

        public Stats(PTC collector, double[] h)
        {
            this.TotalLossCoefficient = iteractiveProcess(collector);
            this.EfficiencyFactor = 1 / (h[0] + h[1] + h[2] + h[3]);
        }

        ///<summary>
        ///Statistics constructor instantiates an Stats object that contains the main values in the Thermal Analysis object of this project
        ///</summary>
        ///<param name="collector">The collector which the user wants to obtain the values</param>
        ///<param name="h">double array containing the h values of the Rankine Cycle</param>
        public Stats(PTC collector, double[] h, double? fluidFlowRate, double? fluidSpecificHeat)
        {
            this.TotalLossCoefficient = iteractiveProcess(collector);
            this.EfficiencyFactor = 1 / (h[0] + h[1] + h[2] + h[3]);
            if (fluidFlowRate != null && fluidSpecificHeat != null)
            {
                this.HeatRemovalFactor = ((double)fluidFlowRate * (double)fluidSpecificHeat) / ((Math.PI * collector.OuterRecieverDiameter * collector.Length) * this.TotalLossCoefficient * this.EfficiencyFactor);
                this.FlowFactor = this.HeatRemovalFactor / this.EfficiencyFactor;
            }
        }

        ///<summary>
        ///Estimates the temperature of the reciever and adjust the error of the Qloss equations on each iteraction till it finds an acceptable error
        ///</summary>
        private double iteractiveProcess(PTC c)
        {
            int maxIterations = 30;
            double qlossError = 0.001; //Acceptable variance between Qloss values (works as stop condition in the algorithm)
            double sigma = 5.67 * Math.Pow(10, -8); //Stefan-Boltzmann Constant
            double coverTemp = (new Random().NextDouble() * c.AmbientTemp) + c.AmbientTemp;
            double qLoss1 = (Math.PI * c.OuterCoverDiameter * c.Length * c.WindConvectiveCoefficient *
            (coverTemp - c.AmbientTemp)) + (c.CoverEmmitance * Math.PI * c.OuterCoverDiameter * c.Length * sigma
            * (Math.Pow(coverTemp, 4) * Math.Pow(c.SkyTemp, 4)));
            double innerCoverTemp = coverTemp + ((qLoss1 * (Math.Log(c.OuterCoverDiameter / c.InnerCoverDiameter)))
                                    / (2 * Math.PI * c.CoverThermalConductivity * c.Length));
            //Calcular temperatura do receptor

            double qLoss2 = (Math.PI * c.OuterRecieverDiameter * c.Length * sigma *
            (Math.Pow(c.RecieverTemp, 4) - Math.Pow(innerCoverTemp, 4))) /
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
                qLoss2 = (Math.PI * c.OuterRecieverDiameter * c.Length * sigma * (Math.Pow(c.RecieverTemp, 4)
                - Math.Pow(innerCoverTemp, 4)));
                maxIterations--;
            }

            return (qLoss2 / (Math.PI * c.OuterRecieverDiameter * c.Length * (c.RecieverTemp - c.AmbientTemp)));
        }
    }
}