﻿using System;
using System.Collections.Generic;

namespace Glicko2
{    
    public static class GlickoCalculator
    {
        public static double VolatilityChange = 0.8;
        public static double ConvergenceTolerance = 0.000001;
        public static double CalulateNewVolatility(GlickoPlayer competitor, double rankingChange, double rankDeviation, double variance)
        {
            var A = VolatilityTransform(competitor.Volatility);
            var a = VolatilityTransform(competitor.Volatility);
            double B = 0.0;

            if (Math.Pow(rankingChange, 2) > (Math.Pow(competitor.GlickoRatingDeviation, 2) + variance))
            {
                B = Math.Log(Math.Pow(rankingChange, 2) - Math.Pow(competitor.GlickoRatingDeviation, 2) - variance);
            }

            if (Math.Pow(rankingChange, 2) <= (Math.Pow(competitor.GlickoRatingDeviation, 2) + variance))
            {
                var k = 1;
                var x = Math.Log(competitor.Volatility - (k * VolatilityChange));

                while(VolatilityFunction(x, rankingChange, rankDeviation, variance, competitor.Volatility) < 0)
                {
                    k++;
                }

                B = VolatilityTransform(competitor.Volatility) - (k * VolatilityChange);
            }

            var fA = VolatilityFunction(A, rankingChange, rankDeviation, variance, competitor.Volatility);
            var fB = VolatilityFunction(B, rankingChange, rankDeviation, variance, competitor.Volatility);

            while (Math.Abs(B - A) > ConvergenceTolerance)
            {
                var C = A + ((A - B) * fA / (fB - fA));
                var fC = VolatilityFunction(C, rankingChange, rankDeviation, variance, competitor.Volatility);

                if ((fC * fB) < 0)
                {
                    A = B;
                    fA = fB;
                }
                else
                {
                    fA = fA / 2;
                }

                B = C;
                fB = fC;
            }

            return Math.Exp(A / 2);
        }

        private static double VolatilityTransform(double volatility)
        {
            return Math.Log(Math.Pow(volatility, 2));
        }

        private static double VolatilityFunction(double x, double rankingChange, double rankDeviation, double variance, double volatility)
        {
            var leftNumerater = Math.Exp(x) * (Math.Pow(rankingChange, 2) - Math.Pow(rankDeviation, 2) - variance - Math.Exp(x));
            var leftDenominator = 2 * Math.Pow(Math.Pow(rankDeviation, 2) + variance + Math.Exp(x), 2);

            var rightNumerater = x - VolatilityTransform(volatility);
            var rightDenomintor = Math.Pow(VolatilityChange, 2);

            return leftNumerater / leftDenominator - rightNumerater / rightDenomintor;
        }

        public static double RatingImprovement(GlickoPlayer competitor, List<GlickoOpponent> opponents)
        {
            double sum = 0;
            var varience = ComputeVariance(competitor, opponents);

            foreach (var opponent in opponents)
            {
                sum += Gphi(opponent.Opponent) * (opponent.Result - Edeltaphi(competitor.GlickoRating, opponent.Opponent));
            }

            return varience * sum;
        }

        public static double ComputeVariance(GlickoPlayer competitor, List<GlickoOpponent> opponents)
        {
            double sum = 0;
            foreach (var opponent in opponents)
            {
                var opponentsGphi = Gphi(opponent.Opponent);

                var eDeltaPhi = Edeltaphi(competitor.GlickoRating, opponent.Opponent);

                sum += Math.Pow(opponentsGphi, 2) * eDeltaPhi * (1 - eDeltaPhi);
            }

            return sum;
        }

        private static double Gphi(GlickoPlayer opponent)
        {
            return 1 / (Math.Sqrt(1 + (3 * Math.Pow(opponent.GlickoRatingDeviation, 2) / Math.Pow(Math.PI, 2))));
        }

        private static double Edeltaphi(double playerRating, GlickoPlayer opponent)
        {
            return 1 / (1 + (Math.Exp(-Gphi(opponent)) * (playerRating - opponent.GlickoRating)));
        }

    }
}