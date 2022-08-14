using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaNs
{
    //enumeration for function codes to select desired final outputs from SPA
    public enum SpaFunction
    {
        ZA,           //calculate zenith and azimuth
        ZA_INC,       //calculate zenith, azimuth, and incidence
        ZA_RTS,       //calculate zenith, azimuth, and sun rise/transit/set values
        ZA_ALL,       //calculate all SPA output values
    };

    public struct Spa_data
    {
        //----------------------INPUT VALUES------------------------

        public int year;            // 4-digit year,      valid range: -2000 to 6000, error code: 1
        public int month;           // 2-digit month,         valid range: 1 to  12,  error code: 2
        public int day;             // 2-digit day,           valid range: 1 to  31,  error code: 3
        public int hour;            // Observer local hour,   valid range: 0 to  24,  error code: 4
        public int minute;          // Observer local minute, valid range: 0 to  59,  error code: 5
        public double second;       // Observer local second, valid range: 0 to <60,  error code: 6

        public double delta_ut1;    // Fractional second difference between UTC and UT which is used
                                    // to adjust UTC for earth's irregular rotation rate and is derived
                                    // from observation only and is reported in this bulletin:
                                    // http://maia.usno.navy.mil/ser7/ser7.dat,
                                    // where delta_ut1 = DUT1
                                    // valid range: -1 to 1 second (exclusive), error code 17

        public double delta_t;      // Difference between earth rotation time and terrestrial time
                                    // It is derived from observation only and is reported in this
                                    // bulletin: http://maia.usno.navy.mil/ser7/ser7.dat,
                                    // where delta_t = 32.184 + (TAI-UTC) - DUT1
                                    // valid range: -8000 to 8000 seconds, error code: 7

        public double timezone;     // Observer time zone (negative west of Greenwich)
                                    // valid range: -18   to   18 hours,   error code: 8

        public double longitude;    // Observer longitude (negative west of Greenwich)
                                    // valid range: -180  to  180 degrees, error code: 9

        public double latitude;     // Observer latitude (negative south of equator)
                                    // valid range: -90   to   90 degrees, error code: 10

        public double elevation;    // Observer elevation [meters]
                                    // valid range: -6500000 or higher meters,    error code: 11

        public double pressure;     // Annual average local pressure [millibars]
                                    // valid range:    0 to 5000 millibars,       error code: 12

        public double temperature;  // Annual average local temperature [degrees Celsius]
                                    // valid range: -273 to 6000 degrees Celsius, error code; 13

        public double slope;        // Surface slope (measured from the horizontal plane)
                                    // valid range: -360 to 360 degrees, error code: 14

        public double azm_rotation; // Surface azimuth rotation (measured from south to projection of
                                    //     surface normal on horizontal plane, negative east)
                                    // valid range: -360 to 360 degrees, error code: 15

        public double atmos_refract;// Atmospheric refraction at sunrise and sunset (0.5667 deg is typical)
                                    // valid range: -5   to   5 degrees, error code: 16

        public SpaFunction function;        // Switch to choose functions for desired output (from enumeration)

        //-----------------Intermediate OUTPUT VALUES--------------------

        public double jd;          //Julian day
        public double jc;          //Julian century

        public double jde;         //Julian ephemeris day
        public double jce;         //Julian ephemeris century
        public double jme;         //Julian ephemeris millennium

        public double l;           //earth heliocentric longitude [degrees]
        public double b;           //earth heliocentric latitude [degrees]
        public double r;           //earth radius vector [Astronomical Units, AU]

        public double theta;       //geocentric longitude [degrees]
        public double beta;        //geocentric latitude [degrees]

        public double x0;          //mean elongation (moon-sun) [degrees]
        public double x1;          //mean anomaly (sun) [degrees]
        public double x2;          //mean anomaly (moon) [degrees]
        public double x3;          //argument latitude (moon) [degrees]
        public double x4;          //ascending longitude (moon) [degrees]

        public double del_psi;     //nutation longitude [degrees]
        public double del_epsilon; //nutation obliquity [degrees]
        public double epsilon0;    //ecliptic mean obliquity [arc seconds]
        public double epsilon;     //ecliptic true obliquity  [degrees]

        public double del_tau;     //aberration correction [degrees]
        public double lamda;       //apparent sun longitude [degrees]
        public double nu0;         //Greenwich mean sidereal time [degrees]
        public double nu;          //Greenwich sidereal time [degrees]

        public double alpha;       //geocentric sun right ascension [degrees]
        public double delta;       //geocentric sun declination [degrees]

        public double h;           //observer hour angle [degrees]
        public double xi;          //sun equatorial horizontal parallax [degrees]
        public double del_alpha;   //sun right ascension parallax [degrees]
        public double delta_prime; //topocentric sun declination [degrees]
        public double alpha_prime; //topocentric sun right ascension [degrees]
        public double h_prime;     //topocentric local hour angle [degrees]

        public double e0;          //topocentric elevation angle (uncorrected) [degrees]
        public double del_e;       //atmospheric refraction correction [degrees]
        public double e;           //topocentric elevation angle (corrected) [degrees]

        public double eot;         //equation of time [minutes]
        public double srha;        //sunrise hour angle [degrees]
        public double ssha;        //sunset hour angle [degrees]
        public double sta;         //sun transit altitude [degrees]

        //---------------------Final OUTPUT VALUES------------------------

        public double zenith;       //topocentric zenith angle [degrees]
        public double azimuth_astro;//topocentric azimuth angle (westward from south) [for astronomers]
        public double azimuth;      //topocentric azimuth angle (eastward from north) [for navigators and solar radiation]
        public double incidence;    //surface incidence angle [degrees]

        public double suntransit;   //local sun transit time (or solar noon) [fractional hour]
        public double sunrise;      //local sunrise time (+/- 30 seconds) [fractional hour]
        public double sunset;       //local sunset time (+/- 30 seconds) [fractional hour]

    }

    class Spa
    {

        const double PI = 3.1415926535897932384626433832795028841971;
        const double SUN_RADIUS = 0.26667;

        const int L_COUNT = 6;
        const int B_COUNT = 2;
        const int R_COUNT = 5;
        const int Y_COUNT = 63;

        const int L_MAX_SUBCOUNT = 64;
        const int B_MAX_SUBCOUNT = 5;
        const int R_MAX_SUBCOUNT = 40;

        const int TERM_Y_COUNT = (int)Term_X.COUNT;

        public Spa_data spa;


        enum Term_alpha { A, B, C, COUNT };
        enum Term_X { X0, X1, X2, X3, X4, COUNT };
        enum Term_press { PSI_A, PSI_B, EPS_C, EPS_D, COUNT };
        enum JulianDay { MINUS, ZERO, PLUS, COUNT };
        enum Sun { TRANSIT, RISE, SET, COUNT };

        private static readonly int[] l_subcount = new int[L_COUNT] { 64, 34, 20, 7, 3, 1 };
        private static readonly int[] b_subcount = new int[B_COUNT] { 5, 2 };
        private static readonly int[] r_subcount = new int[R_COUNT] { 40, 10, 6, 2, 1 };


        ///////////////////////////////////////////////////
        ///  Earth Periodic Terms
        ///////////////////////////////////////////////////
        private static readonly double[][][] L_TERMS = 
        //new double[L_COUNT, L_MAX_SUBCOUNT, (int)Term_alpha.COUNT]
        {
            new double[][]
            {
                new double[] { 175347046.0, 0, 0 },
                new double[] { 3341656.0, 4.6692568, 6283.07585 },
                new double[] { 34894.0, 4.6261, 12566.1517 },
                new double[] { 3497.0, 2.7441, 5753.3849 },
                new double[] { 3418.0, 2.8289, 3.5231 },
                new double[] { 3136.0, 3.6277, 77713.7715 },
                new double[] { 2676.0, 4.4181, 7860.4194 },
                new double[] { 2343.0, 6.1352, 3930.2097 },
                new double[] { 1324.0, 0.7425, 11506.7698 },
                new double[] { 1273.0, 2.0371, 529.691 },
                new double[] { 1199.0, 1.1096, 1577.3435 },
                new double[] { 990, 5.233, 5884.927 },
                new double[] { 902, 2.045, 26.298 },
                new double[] { 857, 3.508, 398.149 },
                new double[] { 780, 1.179, 5223.694 },
                new double[] { 753, 2.533, 5507.553 },
                new double[] { 505, 4.583, 18849.228 },
                new double[] { 492, 4.205, 775.523 },
                new double[] { 357, 2.92, 0.067 },
                new double[] { 317, 5.849, 11790.629 },
                new double[] { 284, 1.899, 796.298 },
                new double[] { 271, 0.315, 10977.079 },
                new double[] { 243, 0.345, 5486.778 },
                new double[] { 206, 4.806, 2544.314 },
                new double[] { 205, 1.869, 5573.143 },
                new double[] { 202, 2.458, 6069.777 },
                new double[] { 156, 0.833, 213.299 },
                new double[] { 132, 3.411, 2942.463 },
                new double[] { 126, 1.083, 20.775 },
                new double[] { 115, 0.645, 0.98 },
                new double[] { 103, 0.636, 4694.003 },
                new double[] { 102, 0.976, 15720.839 },
                new double[] { 102, 4.267, 7.114 },
                new double[] { 99, 6.21, 2146.17 },
                new double[] { 98, 0.68, 155.42 },
                new double[] { 86, 5.98, 161000.69 },
                new double[] { 85, 1.3, 6275.96 },
                new double[] { 85, 3.67, 71430.7 },
                new double[] { 80, 1.81, 17260.15 },
                new double[] {79,3.04,12036.46},
                new double[] {75,1.76,5088.63},
                new double[] {74,3.5,3154.69},
                new double[] {74,4.68,801.82},
                new double[] {70,0.83,9437.76},
                new double[] {62,3.98,8827.39},
                new double[] {61,1.82,7084.9},
                new double[] {57,2.78,6286.6},
                new double[] {56,4.39,14143.5},
                new double[] {56,3.47,6279.55},
                new double[] {52,0.19,12139.55},
                new double[] {52,1.33,1748.02},
                new double[] {51,0.28,5856.48},
                new double[] {49,0.49,1194.45},
                new double[] {41,5.37,8429.24},
                new double[] {41,2.4,19651.05},
                new double[] {39,6.17,10447.39},
                new double[] {37,6.04,10213.29},
                new double[] {37,2.57,1059.38},
                new double[] {36,1.71,2352.87},
                new double[] {36,1.78,6812.77},
                new double[] {33,0.59,17789.85},
                new double[] {30,0.44,83996.85},
                new double[] {30,2.74,1349.87},
                new double[] { 25, 3.16, 4690.48},
            },
            new double[][]{
                new double[] { 628331966747.0,0,0},
                new double[] { 206059.0,2.678235,6283.07585},
                new double[] {4303.0,2.6351,12566.1517},
                new double[] {425.0,1.59,3.523},
                new double[] {119.0,5.796,26.298},
                new double[] {109.0,2.966,1577.344},
                new double[] {93,2.59,18849.23},
                new double[] {72,1.14,529.69},
                new double[] {68,1.87,398.15},
                new double[] {67,4.41,5507.55},
                new double[] {59,2.89,5223.69},
                new double[] {56,2.17,155.42},
                new double[] {45,0.4,796.3},
                new double[] {36,0.47,775.52},
                new double[] {29,2.65,7.11},
                new double[] { 21,5.34,0.98},
                new double[] {19,1.85,5486.78},
                new double[] {19,4.97,213.3},
                new double[] {17,2.99,6275.96},
                new double[] {16,0.03,2544.31},
                new double[] {16,1.43,2146.17},
                new double[] {15,1.21,10977.08},
                new double[] {12,2.83,1748.02},
                new double[] {12,3.26,5088.63},
                new double[] {12,5.27,1194.45},
                new double[] {12,2.08,4694},
                new double[] {11,0.77,553.57},
                new double[] {10,1.3,6286.6},
                new double[] {10,4.24,1349.87},
                new double[] { 9,2.7,242.73},
                new double[] {9,5.64,951.72},
                new double[] {8,5.3,2352.87},
                new double[] {6,2.65,9437.76},
                new double[] { 6, 4.67, 4690.48 },
            },
            new double[][]{
                new double[] { 52919.0,0,0},
                new double[] {8720.0,1.0721,6283.0758},
                new double[] {309.0,0.867,12566.152},
                new double[] {27,0.05,3.52},
                new double[] {16,5.19,26.3},
                new double[] {16,3.68,155.42},
                new double[] {10,0.76,18849.23},
                new double[] {9,2.06,77713.77},
                new double[] {7,0.83,775.52},
                new double[] {5,4.66,1577.34},
                new double[] {4,1.03,7.11},
                new double[] {4,3.44,5573.14},
                new double[] {3,5.14,796.3},
                new double[] {3,6.05,5507.55},
                new double[] {3,1.19,242.73},
                new double[] {3,6.12,529.69},
                new double[] {3,0.31,398.15},
                new double[] {3,2.28,553.57},
                new double[] {2,4.38,5223.69},
                new double[] { 2, 3.75, 0.98 },
            },
            new double[][]{
                new double[] { 289.0,5.844,6283.076},
                new double[] {35,0,0},
                new double[] {17,5.49,12566.15},
                new double[] {3,5.2,155.42},
                new double[] {1,4.72,3.52},
                new double[] {1,5.3,18849.23},
                new double[] { 1, 5.97, 242.73 },
            },
            new double[][]{
                new double[] { 114.0,3.142,0},
                new double[] {8,4.13,6283.08},
                new double[] { 1, 3.84, 12566.15 },
            },
            new double[][]{
                new double[] { 1, 3.14, 0 },
            }
        };

        private static readonly double[][][] B_TERMS =
            //[B_COUNT][B_MAX_SUBCOUNT] [TERM_COUNT]=
        {
            new double[][]{
                new double[] {280.0,3.199,84334.662},
                new double[] {102.0,5.422,5507.553},
                new double[] {80,3.88,5223.69},
                new double[] {44,3.7,2352.87},
                new double[] {32,4,1577.34}
            },
            new double[][]{
                new double[] {9,3.9,5507.55},
                new double[] {6,1.73,5223.69}
            }
        };

        private static readonly double[][][] R_TERMS =
        //[R_COUNT][R_MAX_SUBCOUNT][TERM_COUNT]=
        {
            new double[][]
            {
                new double[] {100013989.0,0,0},
                new double[] {1670700.0,3.0984635,6283.07585},
                new double[] {13956.0,3.05525,12566.1517},
                new double[] {3084.0,5.1985,77713.7715},
                new double[] {1628.0,1.1739,5753.3849},
                new double[] {1576.0,2.8469,7860.4194},
                new double[] {925.0,5.453,11506.77},
                new double[] {542.0,4.564,3930.21},
                new double[] {472.0,3.661,5884.927},
                new double[] {346.0,0.964,5507.553},
                new double[] {329.0,5.9,5223.694},
                new double[] {307.0,0.299,5573.143},
                new double[] {243.0,4.273,11790.629},
                new double[] {212.0,5.847,1577.344},
                new double[] {186.0,5.022,10977.079},
                new double[] {175.0,3.012,18849.228},
                new double[] {110.0,5.055,5486.778},
                new double[] {98,0.89,6069.78},
                new double[] {86,5.69,15720.84},
                new double[] {86,1.27,161000.69},
                new double[] {65,0.27,17260.15},
                new double[] {63,0.92,529.69},
                new double[] {57,2.01,83996.85},
                new double[] {56,5.24,71430.7},
                new double[] {49,3.25,2544.31},
                new double[] {47,2.58,775.52},
                new double[] {45,5.54,9437.76},
                new double[] {43,6.01,6275.96},
                new double[] {39,5.36,4694},
                new double[] {38,2.39,8827.39},
                new double[] {37,0.83,19651.05},
                new double[] {37,4.9,12139.55},
                new double[] {36,1.67,12036.46},
                new double[] {35,1.84,2942.46},
                new double[] {33,0.24,7084.9},
                new double[] {32,0.18,5088.63},
                new double[] {32,1.78,398.15},
                new double[] {28,1.21,6286.6},
                new double[] {28,1.9,6279.55},
                new double[] {26,4.59,10447.39}
                },
            new double[][]
            {
                new double[] {103019.0,1.10749,6283.07585},
                new double[] {1721.0,1.0644,12566.1517},
                new double[] {702.0,3.142,0},
                new double[] {32,1.02,18849.23},
                new double[] {31,2.84,5507.55},
                new double[] {25,1.32,5223.69},
                new double[] {18,1.42,1577.34},
                new double[] {10,5.91,10977.08},
                new double[] {9,1.42,6275.96},
                new double[] {9,0.27,5486.78}
            },
            new double[][]
            {
                new double[] {4359.0,5.7846,6283.0758},
                new double[] {124.0,5.579,12566.152},
                new double[] {12,3.14,0},
                new double[] {9,3.63,77713.77},
                new double[] {6,1.87,5573.14},
                new double[] {3,5.47,18849.23}
            },
            new double[][]
            {
                new double[] {145.0,4.273,6283.076},
                new double[] {7,3.92,12566.15}
            },
            new double[][]
            {
                new double[] {4,2.56,6283.08}
            }
        };

        ////////////////////////////////////////////////////////////////
        ///  Periodic Terms for the nutation in longitude and obliquity
        ////////////////////////////////////////////////////////////////

        private static readonly int[][] Y_TERMS =
        //[Y_COUNT][TERM_Y_COUNT]=
        {
            new int[] {0,0,0,0,1},
            new int[] {-2,0,0,2,2},
            new int[] {0,0,0,2,2},
            new int[] {0,0,0,0,2},
            new int[] {0,1,0,0,0},
            new int[] {0,0,1,0,0},
            new int[] {-2,1,0,2,2},
            new int[] {0,0,0,2,1},
            new int[] {0,0,1,2,2},
            new int[] {-2,-1,0,2,2},
            new int[] {-2,0,1,0,0},
            new int[] {-2,0,0,2,1},
            new int[] {0,0,-1,2,2},
            new int[] {2,0,0,0,0},
            new int[] {0,0,1,0,1},
            new int[] {2,0,-1,2,2},
            new int[] {0,0,-1,0,1},
            new int[] {0,0,1,2,1},
            new int[] {-2,0,2,0,0},
            new int[] {0,0,-2,2,1},
            new int[] {2,0,0,2,2},
            new int[] {0,0,2,2,2},
            new int[] {0,0,2,0,0},
            new int[] {-2,0,1,2,2},
            new int[] {0,0,0,2,0},
            new int[] {-2,0,0,2,0},
            new int[] {0,0,-1,2,1},
            new int[] {0,2,0,0,0},
            new int[] {2,0,-1,0,1},
            new int[] {-2,2,0,2,2},
            new int[] {0,1,0,0,1},
            new int[] {-2,0,1,0,1},
            new int[] {0,-1,0,0,1},
            new int[] {0,0,2,-2,0},
            new int[] {2,0,-1,2,1},
            new int[] {2,0,1,2,2},
            new int[] {0,1,0,2,2},
            new int[] {-2,1,1,0,0},
            new int[] {0,-1,0,2,2},
            new int[] {2,0,0,2,1},
            new int[] {2,0,1,0,0},
            new int[] {-2,0,2,2,2},
            new int[] {-2,0,1,2,1},
            new int[] {2,0,-2,0,1},
            new int[] {2,0,0,0,1},
            new int[] {0,-1,1,0,0},
            new int[] {-2,-1,0,2,1},
            new int[] {-2,0,0,0,1},
            new int[] {0,0,2,2,1},
            new int[] {-2,0,2,0,1},
            new int[] {-2,1,0,2,1},
            new int[] {0,0,1,-2,0},
            new int[] {-1,0,1,0,0},
            new int[] {-2,1,0,0,0},
            new int[] {1,0,0,0,0},
            new int[] {0,0,1,2,0},
            new int[] {0,0,-2,2,2},
            new int[] {-1,-1,1,0,0},
            new int[] {0,1,1,0,0},
            new int[] {0,-1,1,2,2},
            new int[] {2,-1,-1,2,2},
            new int[] {0,0,3,2,2},
            new int[] {2,-1,0,2,2}
        };

        private static readonly double[][] PE_TERMS =
        //[Y_COUNT][TERM_PE_COUNT]=
        {
            new double[] {-171996,-174.2,92025,8.9},
            new double[] {-13187,-1.6,5736,-3.1},
            new double[] {-2274,-0.2,977,-0.5},
            new double[] {2062,0.2,-895,0.5},
            new double[] {1426,-3.4,54,-0.1},
            new double[] {712,0.1,-7,0},
            new double[] {-517,1.2,224,-0.6},
            new double[] {-386,-0.4,200,0},
            new double[] {-301,0,129,-0.1},
            new double[] {217,-0.5,-95,0.3},
            new double[] {-158,0,0,0},
            new double[] {129,0.1,-70,0},
            new double[] {123,0,-53,0},
            new double[] {63,0,0,0},
            new double[] {63,0.1,-33,0},
            new double[] {-59,0,26,0},
            new double[] {-58,-0.1,32,0},
            new double[] {-51,0,27,0},
            new double[] {48,0,0,0},
            new double[] {46,0,-24,0},
            new double[] {-38,0,16,0},
            new double[] {-31,0,13,0},
            new double[] {29,0,0,0},
            new double[] {29,0,-12,0},
            new double[] {26,0,0,0},
            new double[] {-22,0,0,0},
            new double[] {21,0,-10,0},
            new double[] {17,-0.1,0,0},
            new double[] {16,0,-8,0},
            new double[] {-16,0.1,7,0},
            new double[] {-15,0,9,0},
            new double[] {-13,0,7,0},
            new double[] {-12,0,6,0},
            new double[] {11,0,0,0},
            new double[] {-10,0,5,0},
            new double[] {-8,0,3,0},
            new double[] {7,0,-3,0},
            new double[] {-7,0,0,0},
            new double[] {-7,0,3,0},
            new double[] {-7,0,3,0},
            new double[] {6,0,0,0},
            new double[] {6,0,-3,0},
            new double[] {6,0,-3,0},
            new double[] {-6,0,3,0},
            new double[] {-6,0,3,0},
            new double[] {5,0,0,0},
            new double[] {-5,0,3,0},
            new double[] {-5,0,3,0},
            new double[] {-5,0,3,0},
            new double[] {4,0,0,0},
            new double[] {4,0,0,0},
            new double[] {4,0,0,0},
            new double[] {-4,0,0,0},
            new double[] {-4,0,0,0},
            new double[] {-4,0,0,0},
            new double[] {3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
            new double[] {-3,0,0,0},
        };

        ///////////////////////////////////////////////

        double Rad2deg(double radians)
        {
            return (180.0 / PI) * radians;
        }

        double Deg2rad(double degrees)
        {
            return (PI / 180.0) * degrees;
        }

        double Limit_degrees(double degrees)
        {
            double limited;

            degrees /= 360.0;
            limited = 360.0 * (degrees - Math.Floor(degrees));
            if (limited < 0) limited += 360.0;

            return limited;
        }

        double Limit_degrees180pm(double degrees)
        {
            double limited;

            degrees /= 360.0;
            limited = 360.0 * (degrees - Math.Floor(degrees));
            if (limited < -180.0) limited += 360.0;
            else if (limited > 180.0) limited -= 360.0;

            return limited;
        }

        double Limit_degrees180(double degrees)
        {
            double limited;

            degrees /= 180.0;
            limited = 180.0 * (degrees - Math.Floor(degrees));
            if (limited < 0) limited += 180.0;

            return limited;
        }

        double Limit_zero2one(double value)
        {
            double limited;

            limited = value - Math.Floor(value);
            if (limited < 0) limited += 1.0;

            return limited;
        }

        double Limit_minutes(double minutes)
        {
            double limited = minutes;

            if (limited < -20.0) limited += 1440.0;
            else if (limited > 20.0) limited -= 1440.0;

            return limited;
        }

        double Dayfrac_to_local_hr(double dayfrac, double timezone)
        {
            return 24.0 * Limit_zero2one(dayfrac + timezone / 24.0);
        }

        double Third_order_polynomial(double a, double b, double c, double d, double x)
        {
            return ((a * x + b) * x + c) * x + d;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        private int Validate_inputs(Spa_data spa)
        {
            if ((spa.year < -2000) || (spa.year > 6000)) return 1;
            if ((spa.month < 1) || (spa.month > 12)) return 2;
            if ((spa.day < 1) || (spa.day > 31)) return 3;
            if ((spa.hour < 0) || (spa.hour > 24)) return 4;
            if ((spa.minute < 0) || (spa.minute > 59)) return 5;
            if ((spa.second < 0) || (spa.second >= 60)) return 6;
            if ((spa.pressure < 0) || (spa.pressure > 5000)) return 12;
            if ((spa.temperature <= -273) || (spa.temperature > 6000)) return 13;
            if ((spa.delta_ut1 <= -1) || (spa.delta_ut1 >= 1)) return 17;
            if ((spa.hour == 24) && (spa.minute > 0)) return 5;
            if ((spa.hour == 24) && (spa.second > 0)) return 6;

            if (Math.Abs(spa.delta_t) > 8000) return 7;
            if (Math.Abs(spa.timezone) > 18) return 8;
            if (Math.Abs(spa.longitude) > 180) return 9;
            if (Math.Abs(spa.latitude) > 90) return 10;
            if (Math.Abs(spa.atmos_refract) > 5) return 16;
            if (spa.elevation < -6500000) return 11;

            if ((spa.function == SpaFunction.ZA_INC) || (spa.function == SpaFunction.ZA_ALL))
            {
                if (Math.Abs(spa.slope) > 360) return 14;
                if (Math.Abs(spa.azm_rotation) > 360) return 15;
            }

            return 0;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        private double Julian_day(int year, int month, int day, int hour, int minute, double second, double dut1, double tz)
        {
            double day_decimal, julian_day, a;

            day_decimal = day + (hour - tz + (minute + (second + dut1) / 60.0) / 60.0) / 24.0;

            if (month < 3)
            {
                month += 12;
                year--;
            }

            julian_day = (int)(365.25 * (year + 4716.0)) + (int)(30.6001 * (month + 1)) + day_decimal - 1524.5;

            if (julian_day > 2299160.0)
            {
                a = (year / 100);
                julian_day += (2 - a + (a / 4));
            }

            return julian_day;
        }

        double Julian_century(double jd)
        {
            return (jd - 2451545.0) / 36525.0;
        }

        double Julian_ephemeris_day(double jd, double delta_t)
        {
            return jd + delta_t / 86400.0;
        }

        double Julian_ephemeris_century(double jde)
        {
            return (jde - 2451545.0) / 36525.0;
        }

        double Julian_ephemeris_millennium(double jce)
        {
            return (jce / 10.0);
        }

        double Earth_periodic_term_summation(double[][] terms, int count, double jme)
        {
            int i;
            double sum = 0;

            for (i = 0; i < count; i++)
                sum += terms[i][(int)Term_alpha.A] * Math.Cos(terms[i][(int)Term_alpha.B] + terms[i][(int)Term_alpha.C] * jme);

            return sum;
        }

        double Earth_values(double[] term_sum, int count, double jme)
        {
            int i;
            double sum = 0;

            for (i = 0; i < count; i++)
                sum += term_sum[i] * Math.Pow(jme, i);

            sum /= 1.0e8;

            return sum;
        }

        double Earth_heliocentric_longitude(double jme)
        {
            double[] sum = new double[L_COUNT];

            for (int i = 0; i < L_COUNT; i++)
                sum[i] = Earth_periodic_term_summation(L_TERMS[i], l_subcount[i], jme);

            return Limit_degrees(Rad2deg(Earth_values(sum, L_COUNT, jme)));

        }

        double Earth_heliocentric_latitude(double jme)
        {
            double[] sum = new double[B_COUNT];
            int i;

            for (i = 0; i < B_COUNT; i++)
                sum[i] = Earth_periodic_term_summation(B_TERMS[i], b_subcount[i], jme);

            return Rad2deg(Earth_values(sum, B_COUNT, jme));

        }

        double Earth_radius_vector(double jme)
        {
            double[] sum = new double[R_COUNT];
            int i;

            for (i = 0; i < R_COUNT; i++)
                sum[i] = Earth_periodic_term_summation(R_TERMS[i], r_subcount[i], jme);

            return Earth_values(sum, R_COUNT, jme);

        }

        double Geocentric_longitude(double l)
        {
            double theta = l + 180.0;

            if (theta >= 360.0) theta -= 360.0;

            return theta;
        }

        double Geocentric_latitude(double b)
        {
            return -b;
        }

        double Mean_elongation_moon_sun(double jce)
        {
            return Third_order_polynomial(1.0 / 189474.0, -0.0019142, 445267.11148, 297.85036, jce);
        }

        double Mean_anomaly_sun(double jce)
        {
            return Third_order_polynomial(-1.0 / 300000.0, -0.0001603, 35999.05034, 357.52772, jce);
        }

        double Mean_anomaly_moon(double jce)
        {
            return Third_order_polynomial(1.0 / 56250.0, 0.0086972, 477198.867398, 134.96298, jce);
        }

        double Argument_latitude_moon(double jce)
        {
            return Third_order_polynomial(1.0 / 327270.0, -0.0036825, 483202.017538, 93.27191, jce);
        }

        double Ascending_longitude_moon(double jce)
        {
            return Third_order_polynomial(1.0 / 450000.0, 0.0020708, -1934.136261, 125.04452, jce);
        }

        double Xy_term_summation(int i, double[] x)
        {
            int j;
            double sum = 0;

            for (j = 0; j < TERM_Y_COUNT; j++)
                sum += x[j] * Y_TERMS[i][j];

            return sum;
        }

        void Nutation_longitude_and_obliquity(double jce, double[] x, ref double del_psi, ref double del_epsilon)
        {
            int i;
            double xy_term_sum, sum_psi = 0, sum_epsilon = 0;

            for (i = 0; i < Y_COUNT; i++)
            {
                xy_term_sum = Deg2rad(Xy_term_summation(i, x));
                sum_psi += (PE_TERMS[i][(int)Term_press.PSI_A] + jce * PE_TERMS[i][(int)Term_press.PSI_B]) * Math.Sin(xy_term_sum);
                sum_epsilon += (PE_TERMS[i][(int)Term_press.EPS_C] + jce * PE_TERMS[i][(int)Term_press.EPS_D]) * Math.Cos(xy_term_sum);
            }

            del_psi = sum_psi / 36000000.0;
            del_epsilon = sum_epsilon / 36000000.0;
        }

        double Ecliptic_mean_obliquity(double jme)
        {
            double u = jme / 10.0;

            return 84381.448 + u * (-4680.93 + u * (-1.55 + u * (1999.25 + u * (-51.38 + u * (-249.67 +
                               u * (-39.05 + u * (7.12 + u * (27.87 + u * (5.79 + u * 2.45)))))))));
        }

        double Ecliptic_true_obliquity(double delta_epsilon, double epsilon0)
        {
            return delta_epsilon + epsilon0 / 3600.0;
        }

        double Aberration_correction(double r)
        {
            return -20.4898 / (3600.0 * r);
        }

        double Apparent_sun_longitude(double theta, double delta_psi, double delta_tau)
        {
            return theta + delta_psi + delta_tau;
        }

        double Greenwich_mean_sidereal_time(double jd, double jc)
        {
            return Limit_degrees(280.46061837 + 360.98564736629 * (jd - 2451545.0) +
                                               jc * jc * (0.000387933 - jc / 38710000.0));
        }

        double Greenwich_sidereal_time(double nu0, double delta_psi, double epsilon)
        {
            return nu0 + delta_psi * Math.Cos(Deg2rad(epsilon));
        }

        double Geocentric_right_ascension(double lamda, double epsilon, double beta)
        {
            double lamda_rad = Deg2rad(lamda);
            double epsilon_rad = Deg2rad(epsilon);

            return Limit_degrees(Rad2deg(Math.Atan2(Math.Sin(lamda_rad) * Math.Cos(epsilon_rad) -
                                               Math.Tan(Deg2rad(beta)) * Math.Sin(epsilon_rad), Math.Cos(lamda_rad))));
        }

        double Geocentric_declination(double beta, double epsilon, double lamda)
        {
            double beta_rad = Deg2rad(beta);
            double epsilon_rad = Deg2rad(epsilon);

            return Rad2deg(Math.Asin(Math.Sin(beta_rad) * Math.Cos(epsilon_rad) +
                                Math.Cos(beta_rad) * Math.Sin(epsilon_rad) * Math.Sin(Deg2rad(lamda))));
        }

        double Observer_hour_angle(double nu, double longitude, double alpha_deg)
        {
            return Limit_degrees(nu + longitude - alpha_deg);
        }

        double Sun_equatorial_horizontal_parallax(double r)
        {
            return 8.794 / (3600.0 * r);
        }

        void Right_ascension_parallax_and_topocentric_dec(double latitude, double elevation,
           double xi, double h, double delta, ref double delta_alpha, ref double delta_prime)
        {
            double delta_alpha_rad;
            double lat_rad = Deg2rad(latitude);
            double xi_rad = Deg2rad(xi);
            double h_rad = Deg2rad(h);
            double delta_rad = Deg2rad(delta);
            double u = Math.Atan(0.99664719 * Math.Tan(lat_rad));
            double y = 0.99664719 * Math.Sin(u) + elevation * Math.Sin(lat_rad) / 6378140.0;
            double x = Math.Cos(u) + elevation * Math.Cos(lat_rad) / 6378140.0;

            delta_alpha_rad = Math.Atan2(-x * Math.Sin(xi_rad) * Math.Sin(h_rad),
                                          Math.Cos(delta_rad) - x * Math.Sin(xi_rad) * Math.Cos(h_rad));

            delta_prime = Rad2deg(Math.Atan2((Math.Sin(delta_rad) - y * Math.Sin(xi_rad)) * Math.Cos(delta_alpha_rad),
                                          Math.Cos(delta_rad) - x * Math.Sin(xi_rad) * Math.Cos(h_rad)));

            delta_alpha = Rad2deg(delta_alpha_rad);
        }

        double Topocentric_right_ascension(double alpha_deg, double delta_alpha)
        {
            return alpha_deg + delta_alpha;
        }

        double Topocentric_local_hour_angle(double h, double delta_alpha)
        {
            return h - delta_alpha;
        }

        double Topocentric_elevation_angle(double latitude, double delta_prime, double h_prime)
        {
            double lat_rad = Deg2rad(latitude);
            double delta_prime_rad = Deg2rad(delta_prime);

            return Rad2deg(Math.Asin(Math.Sin(lat_rad) * Math.Sin(delta_prime_rad) +
                                Math.Cos(lat_rad) * Math.Cos(delta_prime_rad) * Math.Cos(Deg2rad(h_prime))));
        }

        double Atmospheric_refraction_correction(double pressure, double temperature, double atmos_refract, double e0)
        {
            double del_e = 0;

            if (e0 >= -1 * (SUN_RADIUS + atmos_refract))
                del_e = (pressure / 1010.0) * (283.0 / (273.0 + temperature)) *
                         1.02 / (60.0 * Math.Tan(Deg2rad(e0 + 10.3 / (e0 + 5.11))));

            return del_e;
        }

        double Topocentric_elevation_angle_corrected(double e0, double delta_e)
        {
            return e0 + delta_e;
        }

        double Topocentric_zenith_angle(double e)
        {
            return 90.0 - e;
        }

        double Topocentric_azimuth_angle_astro(double h_prime, double latitude, double delta_prime)
        {
            double h_prime_rad = Deg2rad(h_prime);
            double lat_rad = Deg2rad(latitude);

            return Limit_degrees(Rad2deg(Math.Atan2(Math.Sin(h_prime_rad),
                                 Math.Cos(h_prime_rad) * Math.Sin(lat_rad) - Math.Tan(Deg2rad(delta_prime)) * Math.Cos(lat_rad))));
        }

        double Topocentric_azimuth_angle(double azimuth_astro)
        {
            return Limit_degrees(azimuth_astro + 180.0);
        }

        double Surface_incidence_angle(double zenith, double azimuth_astro, double azm_rotation, double slope)
        {
            double zenith_rad = Deg2rad(zenith);
            double slope_rad = Deg2rad(slope);

            return Rad2deg(Math.Acos(Math.Cos(zenith_rad) * Math.Cos(slope_rad) +
                                Math.Sin(slope_rad) * Math.Sin(zenith_rad) * Math.Cos(Deg2rad(azimuth_astro - azm_rotation))));
        }

        double Sun_mean_longitude(double jme)
        {
            return Limit_degrees(280.4664567 + jme * (360007.6982779 + jme * (0.03032028 +
                            jme * (1 / 49931.0 + jme * (-1 / 15300.0 + jme * (-1 / 2000000.0))))));
        }

        double eot(double m, double alpha, double del_psi, double epsilon)
        {
            return Limit_minutes(4.0 * (m - 0.0057183 - alpha + del_psi * Math.Cos(Deg2rad(epsilon))));
        }

        double Approx_sun_transit_time(double alpha_zero, double longitude, double nu)
        {
            return (alpha_zero - longitude - nu) / 360.0;
        }

        double Sun_hour_angle_at_rise_set(double latitude, double delta_zero, double h0_prime)
        {
            double h0 = -99999;
            double latitude_rad = Deg2rad(latitude);
            double delta_zero_rad = Deg2rad(delta_zero);
            double argument = (Math.Sin(Deg2rad(h0_prime)) - Math.Sin(latitude_rad) * Math.Sin(delta_zero_rad)) /
                                                             (Math.Cos(latitude_rad) * Math.Cos(delta_zero_rad));

            if (Math.Abs(argument) <= 1) h0 = Limit_degrees180(Rad2deg(Math.Acos(argument)));

            return h0;
        }

        void Approx_sun_rise_and_set(double[] m_rts, double h0)
        {
            double h0_dfrac = h0 / 360.0;

            m_rts[(int)Sun.RISE] = Limit_zero2one(m_rts[(int)Sun.TRANSIT] - h0_dfrac);
            m_rts[(int)Sun.SET] = Limit_zero2one(m_rts[(int)Sun.TRANSIT] + h0_dfrac);
            m_rts[(int)Sun.TRANSIT] = Limit_zero2one(m_rts[(int)Sun.TRANSIT]);
        }

        double Rts_alpha_delta_prime(double[] ad, double n)
        {
            double a = ad[(int)JulianDay.ZERO] - ad[(int)JulianDay.MINUS];
            double b = ad[(int)JulianDay.PLUS] - ad[(int)JulianDay.ZERO];

            if (Math.Abs(a) >= 2.0) a = Limit_zero2one(a);
            if (Math.Abs(b) >= 2.0) b = Limit_zero2one(b);

            return ad[(int)JulianDay.ZERO] + n * (a + b + (b - a) * n) / 2.0;
        }

        double Rts_sun_altitude(double latitude, double delta_prime, double h_prime)
        {
            double latitude_rad = Deg2rad(latitude);
            double delta_prime_rad = Deg2rad(delta_prime);

            return Rad2deg(Math.Asin(Math.Sin(latitude_rad) * Math.Sin(delta_prime_rad) +
                                Math.Cos(latitude_rad) * Math.Cos(delta_prime_rad) * Math.Cos(Deg2rad(h_prime))));
        }

        double Sun_rise_and_set(double[] m_rts, double[] h_rts, double[] delta_prime, double latitude,
                                double[] h_prime, double h0_prime, int sun)
        {
            return m_rts[sun] + (h_rts[sun] - h0_prime) /
                  (360.0 * Math.Cos(Deg2rad(delta_prime[sun])) * Math.Cos(Deg2rad(latitude)) * Math.Sin(Deg2rad(h_prime[sun])));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        // Calculate required SPA parameters to get the right ascension (alpha) and declination (delta)
        // Note: JD must be already calculated and in structure
        ////////////////////////////////////////////////////////////////////////////////////////////////
        void Calculate_geocentric_sun_right_ascension_and_declination(ref Spa_data spa)
        {
            double[] x = new double[(int)Term_X.COUNT];

            spa.jc = Julian_century(spa.jd);

            spa.jde = Julian_ephemeris_day(spa.jd, spa.delta_t);
            spa.jce = Julian_ephemeris_century(spa.jde);
            spa.jme = Julian_ephemeris_millennium(spa.jce);

            spa.l = Earth_heliocentric_longitude(spa.jme);
            spa.b = Earth_heliocentric_latitude(spa.jme);
            spa.r = Earth_radius_vector(spa.jme);

            spa.theta = Geocentric_longitude(spa.l);
            spa.beta = Geocentric_latitude(spa.b);

            x[(int)Term_X.X0] = spa.x0 = Mean_elongation_moon_sun(spa.jce);
            x[(int)Term_X.X1] = spa.x1 = Mean_anomaly_sun(spa.jce);
            x[(int)Term_X.X2] = spa.x2 = Mean_anomaly_moon(spa.jce);
            x[(int)Term_X.X3] = spa.x3 = Argument_latitude_moon(spa.jce);
            x[(int)Term_X.X4] = spa.x4 = Ascending_longitude_moon(spa.jce);

            Nutation_longitude_and_obliquity(spa.jce, x, ref spa.del_psi, ref spa.del_epsilon);

            spa.epsilon0 = Ecliptic_mean_obliquity(spa.jme);
            spa.epsilon = Ecliptic_true_obliquity(spa.del_epsilon, spa.epsilon0);

            spa.del_tau = Aberration_correction(spa.r);
            spa.lamda = Apparent_sun_longitude(spa.theta, spa.del_psi, spa.del_tau);
            spa.nu0 = Greenwich_mean_sidereal_time(spa.jd, spa.jc);
            spa.nu = Greenwich_sidereal_time(spa.nu0, spa.del_psi, spa.epsilon);

            spa.alpha = Geocentric_right_ascension(spa.lamda, spa.epsilon, spa.beta);
            spa.delta = Geocentric_declination(spa.beta, spa.epsilon, spa.lamda);
        }

        ////////////////////////////////////////////////////////////////////////
        // Calculate Equation of Time (EOT) and Sun Rise, Transit, & Set (RTS)
        ////////////////////////////////////////////////////////////////////////

        void Calculate_eot_and_sun_rise_transit_set(ref Spa_data spa)
        {
            Spa_data sun_rts;
            double nu, m, h0, n;
            double[] alpha = new double[(int)JulianDay.COUNT]; double[] delta = new double[(int)JulianDay.COUNT];
            double[] m_rts = new double[(int)Sun.COUNT]; double[] nu_rts = new double[(int)Sun.COUNT]; double[] h_rts = new double[(int)Sun.COUNT];
            double[] alpha_prime = new double[(int)Sun.COUNT]; double[] delta_prime = new double[(int)Sun.COUNT]; double[] h_prime = new double[(int)Sun.COUNT];
            double h0_prime = -1 * (SUN_RADIUS + spa.atmos_refract);
            int i;

            sun_rts = spa;
            m = Sun_mean_longitude(spa.jme);
            spa.eot = eot(m, spa.alpha, spa.del_psi, spa.epsilon);

            sun_rts.hour = sun_rts.minute = 0; sun_rts.second = 0;
            sun_rts.delta_ut1 = sun_rts.timezone = 0.0;

            sun_rts.jd = Julian_day(sun_rts.year, sun_rts.month, sun_rts.day, sun_rts.hour,
                                     sun_rts.minute, sun_rts.second, sun_rts.delta_ut1, sun_rts.timezone);

            Calculate_geocentric_sun_right_ascension_and_declination(ref sun_rts);
            nu = sun_rts.nu;

            sun_rts.delta_t = 0;
            sun_rts.jd--;
            for (i = 0; i < (int)JulianDay.COUNT; i++)
            {
                Calculate_geocentric_sun_right_ascension_and_declination(ref sun_rts);
                alpha[i] = sun_rts.alpha;
                delta[i] = sun_rts.delta;
                sun_rts.jd++;
            }

            m_rts[(int)Sun.TRANSIT] = Approx_sun_transit_time(alpha[(int)JulianDay.ZERO], spa.longitude, nu);
            h0 = Sun_hour_angle_at_rise_set(spa.latitude, delta[(int)JulianDay.ZERO], h0_prime);

            if (h0 >= 0)
            {

                Approx_sun_rise_and_set(m_rts, h0);

                for (i = 0; i < (int)Sun.COUNT; i++)
                {

                    nu_rts[i] = nu + 360.985647 * m_rts[i];

                    n = m_rts[i] + spa.delta_t / 86400.0;
                    alpha_prime[i] = Rts_alpha_delta_prime(alpha, n);
                    delta_prime[i] = Rts_alpha_delta_prime(delta, n);

                    h_prime[i] = Limit_degrees180pm(nu_rts[i] + spa.longitude - alpha_prime[i]);

                    h_rts[i] = Rts_sun_altitude(spa.latitude, delta_prime[i], h_prime[i]);
                }

                spa.srha = h_prime[(int)Sun.RISE];
                spa.ssha = h_prime[(int)Sun.SET];
                spa.sta = h_rts[(int)Sun.TRANSIT];

                spa.suntransit = Dayfrac_to_local_hr(m_rts[(int)Sun.TRANSIT] - h_prime[(int)Sun.TRANSIT] / 360.0,
                                                      spa.timezone);

                spa.sunrise = Dayfrac_to_local_hr(Sun_rise_and_set(m_rts, h_rts, delta_prime,
                                  spa.latitude, h_prime, h0_prime, (int)Sun.RISE), spa.timezone);

                spa.sunset = Dayfrac_to_local_hr(Sun_rise_and_set(m_rts, h_rts, delta_prime,
                                  spa.latitude, h_prime, h0_prime, (int)Sun.SET), spa.timezone);

            }
            else spa.srha = spa.ssha = spa.sta = spa.suntransit = spa.sunrise = spa.sunset = -99999;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Calculate all SPA parameters and put into structure
        // Note: All inputs values (listed in header file) must already be in structure
        ///////////////////////////////////////////////////////////////////////////////////////////
        public int Spa_calculate()
        {
            int result;

            result = Validate_inputs(spa);

            if (result == 0)
            {
                spa.jd = Julian_day(spa.year, spa.month, spa.day, spa.hour,
                                      spa.minute, spa.second, spa.delta_ut1, spa.timezone);
            }

            Calculate_geocentric_sun_right_ascension_and_declination(ref spa);

            spa.h = Observer_hour_angle(spa.nu, spa.longitude, spa.alpha);
            spa.xi = Sun_equatorial_horizontal_parallax(spa.r);

            Right_ascension_parallax_and_topocentric_dec(spa.latitude, spa.elevation, spa.xi,
                                    spa.h, spa.delta, ref spa.del_alpha, ref spa.delta_prime);

            spa.alpha_prime = Topocentric_right_ascension(spa.alpha, spa.del_alpha);
            spa.h_prime = Topocentric_local_hour_angle(spa.h, spa.del_alpha);

            spa.e0 = Topocentric_elevation_angle(spa.latitude, spa.delta_prime, spa.h_prime);
            spa.del_e = Atmospheric_refraction_correction(spa.pressure, spa.temperature, spa.atmos_refract, spa.e0);
            spa.e = Topocentric_elevation_angle_corrected(spa.e0, spa.del_e);

            spa.zenith = Topocentric_zenith_angle(spa.e);
            spa.azimuth_astro = Topocentric_azimuth_angle_astro(spa.h_prime, spa.latitude, spa.delta_prime);
            spa.azimuth = Topocentric_azimuth_angle(spa.azimuth_astro);

            if ((spa.function == SpaFunction.ZA_INC) || (spa.function == SpaFunction.ZA_ALL))
                spa.incidence = Surface_incidence_angle(spa.zenith, spa.azimuth_astro, spa.azm_rotation, spa.slope);

            if ((spa.function == SpaFunction.ZA_RTS) || (spa.function == SpaFunction.ZA_ALL))
                Calculate_eot_and_sun_rise_transit_set(ref spa);

            return result;
        }

    }
}

