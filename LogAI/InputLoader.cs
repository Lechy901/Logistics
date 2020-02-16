using System;
using System.Collections.Generic;
using System.Text;

namespace LogAI {
    class InputLoader {

        enum LoadStatus {
            CITYNUM,
            PLACESNUM,
            PLACES,
            AIRPORTS,
            VANSNUM,
            VANS,
            PLANESNUM,
            PLANES,
            PACKAGESNUM,
            PACKAGES,
            FINISHED
        };

        // loads input from file into GlobalInfo and returns the starting state
        public State LoadInput(string filepath, bool opt) {
            LoadStatus loadStatus = LoadStatus.CITYNUM;
            State r = new State();

            int curSubIter = 0;
            string[] lines = System.IO.File.ReadAllLines(filepath);
            foreach(var line in lines) {
                if (line.StartsWith('%')) {
                    continue;
                }
                int firstNumber = -1;
                int secondNumber = -1;
                if (line.Contains(' ')) {
                    string[] numbers = line.Split(' ');
                    firstNumber = int.Parse(numbers[0]);
                    secondNumber = int.Parse(numbers[1]);
                } else {
                    firstNumber = int.Parse(line);
                }
                
                switch(loadStatus) {
                    case LoadStatus.CITYNUM:
                        GlobalInfo.citiesNumber = firstNumber;
                        GlobalInfo.airportPlaces = new int[firstNumber];
                        loadStatus = LoadStatus.PLACESNUM;
                        break;
                    case LoadStatus.PLACESNUM:
                        GlobalInfo.placesNumber = firstNumber;
                        GlobalInfo.placesCities = new int[firstNumber];
                        loadStatus = LoadStatus.PLACES;
                        break;
                    case LoadStatus.PLACES:
                        GlobalInfo.placesCities[curSubIter++] = firstNumber;
                        if (curSubIter >= GlobalInfo.placesNumber) {
                            curSubIter = 0;
                            loadStatus = LoadStatus.AIRPORTS;
                        }
                        break;
                    case LoadStatus.AIRPORTS:
                        GlobalInfo.airportPlaces[curSubIter++] = firstNumber;
                        if (curSubIter >= GlobalInfo.citiesNumber) {
                            curSubIter = 0;
                            loadStatus = LoadStatus.VANSNUM;
                        }
                        break;
                    case LoadStatus.VANSNUM:
                        GlobalInfo.vansNumber = firstNumber;
                        r.Vans = new int[firstNumber];
                        loadStatus = LoadStatus.VANS;
                        break;
                    case LoadStatus.VANS:
                        r.Vans[curSubIter++] = firstNumber;
                        if (curSubIter >= GlobalInfo.vansNumber) {
                            curSubIter = 0;
                            loadStatus = LoadStatus.PLANESNUM;
                        }
                        break;
                    case LoadStatus.PLANESNUM:
                        GlobalInfo.planesNumber = firstNumber;
                        r.Planes = new int[firstNumber];
                        loadStatus = LoadStatus.PLANES;
                        break;
                    case LoadStatus.PLANES:
                        r.Planes[curSubIter++] = firstNumber;
                        if (curSubIter >= GlobalInfo.planesNumber) {
                            curSubIter = 0;
                            loadStatus = LoadStatus.PACKAGESNUM;
                        }
                        break;
                    case LoadStatus.PACKAGESNUM:
                        GlobalInfo.packagesNumber = firstNumber;
                        r.Packages = new int[firstNumber];
                        GlobalInfo.packageDestinations = new int[firstNumber];
                        r.PackagesVans = new int[firstNumber];
                        for (int i = 0; i < r.PackagesVans.Length; i++) {
                            r.PackagesVans[i] = -1;
                        }
                        r.PackagesPlanes = new int[firstNumber];
                        for (int i = 0; i < r.PackagesPlanes.Length; i++) {
                            r.PackagesPlanes[i] = -1;
                        }
                        loadStatus = LoadStatus.PACKAGES;
                        break;
                    case LoadStatus.PACKAGES:
                        GlobalInfo.packageDestinations[curSubIter] = secondNumber;
                        r.Packages[curSubIter++] = firstNumber;
                        if (curSubIter >= GlobalInfo.packagesNumber) {
                            curSubIter = 0;
                            loadStatus = LoadStatus.FINISHED;
                        }
                        break;
                    case LoadStatus.FINISHED:
                    default:
                        throw new FormatException("Bad format of input");
                }
            }
            r.PriceSoFar = 0;
            r.Parent = null;
            r.Action = null;
            r.CalculateHeuristic(opt);

            return r;
        }
    }
}
