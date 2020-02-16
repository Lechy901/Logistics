using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LogAI {
    class State {
        public int PriceSoFar { get; set; }
        public int Heuristic { get; set; }
        public int[] Vans { get; set; }
        public int[] PackagesVans { get; set; }
        public int[] Packages { get; set; }
        public int[] Planes { get; set; }
        public int[] PackagesPlanes { get; set; }
        public int[] Action { get; set; }
        public State Parent { get; set; }

        public override int GetHashCode() {
            unchecked {
                int hash = 27;
                hash = (13 * hash) + Heuristic.GetHashCode();
                for (int i = 0; i < Vans.Length; i++)
                    hash = (13 * hash) + Vans[i].GetHashCode();
                for (int i = 0; i < PackagesVans.Length; i++)
                    hash = (13 * hash) + PackagesVans[i].GetHashCode();
                for (int i = 0; i < Packages.Length; i++)
                    hash = (13 * hash) + Packages[i].GetHashCode();
                for (int i = 0; i < Planes.Length; i++)
                    hash = (13 * hash) + Planes[i].GetHashCode();
                for (int i = 0; i < PackagesPlanes.Length; i++)
                    hash = (13 * hash) + PackagesPlanes[i].GetHashCode();
                return hash;
            }
        }

        public bool Equals(State other) {
            if (Heuristic != other.Heuristic)
                return false;

            for (int i = 0; i < Vans.Length; i++)
                if (Vans[i] != other.Vans[i])
                    return false;

            for (int i = 0; i < PackagesVans.Length; i++)
                if (PackagesVans[i] != other.PackagesVans[i])
                    return false;

            for (int i = 0; i < Packages.Length; i++)
                if (Packages[i] != other.Packages[i])
                    return false;

            for (int i = 0; i < Planes.Length; i++)
                if (Planes[i] != other.Planes[i])
                    return false;

            for (int i = 0; i < PackagesPlanes.Length; i++)
                if (PackagesPlanes[i] != other.PackagesPlanes[i])
                    return false;

            return true;
        }

        public bool IsFinal() {
            return Heuristic == 0;
        }

        // returns a list of all possible next states
        public List<State> GetNextStates(bool opt) {
            List<State> r = new List<State>();

            // van moving
            // for each van, add all possible transitions to another place in the same city
            for(int vanIndex = 0; vanIndex < Vans.Length; vanIndex++) {
                int curPlace = Vans[vanIndex];
                int curCity = GlobalInfo.placesCities[curPlace];
                for(int nextPlace = 0; nextPlace < GlobalInfo.placesNumber; nextPlace++) {
                    if (curCity != GlobalInfo.placesCities[nextPlace]) {
                        // place in another city
                        continue;
                    }
                    if (nextPlace == curPlace) {
                        // the same place as the van is currently in
                        continue;
                    }
                    State next = GetCopy();
                    next.Vans[vanIndex] = nextPlace;
                    // move all packages in the van too
                    for (int packageIndex = 0; packageIndex < Packages.Length; packageIndex++) {
                        if (next.PackagesVans[packageIndex] == vanIndex) {
                            next.Packages[packageIndex] = nextPlace;
                        }
                    }
                    next.PriceSoFar += 17;
                    next.CalculateHeuristic(opt);
                    next.Action = new int[] { 0, vanIndex, nextPlace };
                    next.Parent = this;
                    r.Add(next);
                }
            }

            // van loading
            // for each non-full van, try to load all packages in the place
            for (int vanIndex = 0; vanIndex < Vans.Length; vanIndex++) {
                if (PackagesVans.Where(x => x == vanIndex).Count() >= 4) {
                    // the van is full
                    continue;
                }
                for (int packageIndex = 0; packageIndex < Packages.Length; packageIndex++) {
                    if (PackagesVans[packageIndex] != -1 || PackagesPlanes[packageIndex] != -1) {
                        // the package is already loaded somewhere
                        continue;
                    }
                    if (Packages[packageIndex] != Vans[vanIndex]) {
                        continue;
                    }
                    
                    State next = GetCopy();
                    next.PackagesVans[packageIndex] = vanIndex;
                    next.PriceSoFar += 2;
                    next.CalculateHeuristic(opt);
                    next.Action = new int[] { 1, vanIndex, packageIndex };
                    next.Parent = this;
                    r.Add(next);
                }
            }

            // van unloading
            // for each package in a van, try unloading it
            for(int packageIndex = 0; packageIndex < Packages.Length; packageIndex++) {
                int vanIndex = PackagesVans[packageIndex];
                if (vanIndex == -1) {
                    continue;
                }
                State next = GetCopy();
                next.PackagesVans[packageIndex] = -1;
                next.PriceSoFar += 2;
                next.CalculateHeuristic(opt);
                next.Action = new int[] { 2, vanIndex, packageIndex };
                next.Parent = this;
                r.Add(next);
            }

            // plane flying
            // for each plane, try to fly to any other airport
            for(int planeIndex = 0; planeIndex < Planes.Length; planeIndex++) {
                int curPlace = Planes[planeIndex];
                foreach(int nextAirport in GlobalInfo.airportPlaces) {
                    if (curPlace == nextAirport) {
                        continue;
                    }
                    State next = GetCopy();
                    next.Planes[planeIndex] = nextAirport;
                    for(int packageIndex = 0; packageIndex < Packages.Length; packageIndex++) {
                        if (next.PackagesPlanes[packageIndex] == planeIndex) {
                            next.Packages[packageIndex] = nextAirport;
                        }
                    }
                    next.PriceSoFar += 1000;
                    next.CalculateHeuristic(opt);
                    next.Action = new int[] { 3, planeIndex, nextAirport };
                    next.Parent = this;
                    r.Add(next);
                }
            }

            // plane loading
            // for each non-full plane, try to load all packages in its place
            for (int planeIndex = 0; planeIndex < Planes.Length; planeIndex++) {
                if (PackagesPlanes.Where(x => x == planeIndex).Count() >= 30) {
                    // the plane is full
                    continue;
                }
                for (int packageIndex = 0; packageIndex < Packages.Length; packageIndex++) {
                    if (PackagesVans[packageIndex] != -1 || PackagesPlanes[packageIndex] != -1) {
                        // the package is already loaded somewhere
                        continue;
                    }
                    if (Packages[packageIndex] != Planes[planeIndex]) {
                        continue;
                    }
                    State next = GetCopy();
                    next.PackagesPlanes[packageIndex] = planeIndex;
                    next.PriceSoFar += 14;
                    next.CalculateHeuristic(opt);
                    next.Action = new int[] { 4, planeIndex, packageIndex };
                    next.Parent = this;
                    r.Add(next);
                }
            }

            // plane unloading
            // for each space on a plane, try to unload it if it is occupied
            for (int packageIndex = 0; packageIndex < Packages.Length; packageIndex++) {
                int planeIndex = PackagesPlanes[packageIndex];
                if (planeIndex == -1) {
                    continue;
                }
                State next = GetCopy();
                next.PackagesPlanes[packageIndex] = -1;
                next.PriceSoFar += 11;
                next.CalculateHeuristic(opt);
                next.Action = new int[] { 5, planeIndex, packageIndex };
                next.Parent = this;
                r.Add(next);
            }

            if (!opt) { 
                // remove all next states that increased the heuristic. there is no reason why the heuristic should ever increase in the solution
                r.RemoveAll(x => x.Heuristic > Heuristic);
            }

            return r;
        }

        public void CalculateHeuristic(bool opt) {
            if (opt)
                AdmissibleHeuristic();
            else
                FastHeuristic();
        }

        private void AdmissibleHeuristic() {
            Heuristic = 0;
            for (int packageIndex = 0; packageIndex < Packages.Length; packageIndex++) {
                int packageDestPlace = GlobalInfo.packageDestinations[packageIndex];
                int packageCurPlace = Packages[packageIndex];
                bool isCurrentlyLoadedOnVan = PackagesVans[packageIndex] != -1;
                bool isCurrentlyLoadedOnPlane = PackagesPlanes[packageIndex] != -1;
                if (packageDestPlace == packageCurPlace) {
                    // the package is in the right place, just need to unload it if needed
                    if (isCurrentlyLoadedOnVan) {
                        Heuristic += 2;
                    }
                    if (isCurrentlyLoadedOnPlane) {
                        Heuristic += 11;
                    }
                    continue;
                }
                int packageDestCity = GlobalInfo.placesCities[packageDestPlace];
                int packageCurCity = GlobalInfo.placesCities[packageCurPlace];
                if (packageDestCity == packageCurCity) {
                    // the package is in the wrong place but in the right city
                    Heuristic += 4 + 2;
                    if (!isCurrentlyLoadedOnVan) {
                        Heuristic += 2;
                    }
                    if (isCurrentlyLoadedOnPlane) {
                        Heuristic += 11;
                    }
                    continue;
                }

                // the van is in the wrong city -> we need to fly
                bool isDestAirport = GlobalInfo.airportPlaces.Contains(packageDestPlace);
                bool isCurAirport = GlobalInfo.airportPlaces.Contains(packageCurPlace);

                Heuristic += 33 + 11;
                if (!isCurrentlyLoadedOnPlane) {
                    Heuristic += 14;
                }
                if (!isCurAirport) {
                    // current city is not an airport -> we need to add the price of getting to an airport
                    Heuristic += 4 + 2;
                    if (!isCurrentlyLoadedOnVan) {
                        Heuristic += 2;
                    }
                }
                if (!isDestAirport) {
                    // the destination is not the airport -> we need to add the price of getting there from the airport
                    Heuristic += 4 + 2 + 2;
                }
            }

        }

        private void FastHeuristic() {
            Heuristic = 0;
            for (int packageIndex = 0; packageIndex < Packages.Length; packageIndex++) {
                int packageDestPlace = GlobalInfo.packageDestinations[packageIndex];
                int packageCurPlace = Packages[packageIndex];
                bool isCurrentlyLoadedOnVan = PackagesVans[packageIndex] != -1;
                bool isCurrentlyLoadedOnPlane = PackagesPlanes[packageIndex] != -1;
                if (packageDestPlace == packageCurPlace) {
                    // the package is in the right place, just need to unload it if needed
                    if (isCurrentlyLoadedOnVan) {
                        Heuristic += 2;
                    }
                    if (isCurrentlyLoadedOnPlane) {
                        Heuristic += 11;
                    }
                    continue;
                }
                int packageDestCity = GlobalInfo.placesCities[packageDestPlace];
                int packageCurCity = GlobalInfo.placesCities[packageCurPlace];
                bool curPlaceContainsVan = Vans.Contains(packageCurPlace);
                if (packageDestCity == packageCurCity) {
                    // the package is in the wrong place but in the right city
                    Heuristic += 17 + 2;
                    if (!isCurrentlyLoadedOnVan) {
                        Heuristic += 2;
                    }
                    if (isCurrentlyLoadedOnPlane) {
                        Heuristic += 11;
                    }
                    if (!curPlaceContainsVan) { // we will need to move a van there eventually
                        Heuristic += 17;
                    }
                    continue;
                }

                // the van is in the wrong city -> we need to fly
                bool isDestAirport = GlobalInfo.airportPlaces.Contains(packageDestPlace);
                bool isCurAirport = GlobalInfo.airportPlaces.Contains(packageCurPlace);

                int destCityAirport = -1;
                int curCityAirport = -1;

                if (isDestAirport) {
                    destCityAirport = packageDestPlace;
                } else {
                    destCityAirport = GlobalInfo.airportPlaces.Where(x => GlobalInfo.placesCities[x] == packageDestCity).First();
                }

                if (isCurAirport) {
                    curCityAirport = packageCurPlace;
                } else {
                    curCityAirport = GlobalInfo.airportPlaces.Where(x => GlobalInfo.placesCities[x] == packageCurCity).First();
                }

                bool destCityAirportContainsVan = Vans.Contains(destCityAirport);
                bool curCityAirportContainsPlane = Planes.Contains(curCityAirport);

                if (!curCityAirportContainsPlane) { // we will need to fly a plane here eventually
                    Heuristic += 1000;
                }

                Heuristic += 1000 + 11;
                if (!isCurrentlyLoadedOnPlane) {
                    Heuristic += 14;
                }
                if (!isCurAirport) {
                    // current city is not an airport -> we need to add the price of getting to an airport
                    Heuristic += 17 + 2;
                    if (!isCurrentlyLoadedOnVan) {
                        Heuristic += 2;
                    }
                    if (!curPlaceContainsVan) {
                        Heuristic += 17;
                    }
                }
                if (!isDestAirport) {
                    // the destination is not the airport -> we need to add the price of getting there from the airport
                    Heuristic += 17 + 2 + 2;

                    if (!destCityAirportContainsVan) {
                        Heuristic += 17;
                    }
                }
            }

        }

        // returns a deep copy of the state
        private State GetCopy() {
            State copy = new State();
            copy.PriceSoFar = PriceSoFar;
            copy.Heuristic = Heuristic;
            copy.Vans = new int[GlobalInfo.vansNumber];
            copy.PackagesVans = new int[GlobalInfo.packagesNumber];
            copy.Packages = new int[GlobalInfo.packagesNumber];
            copy.Planes = new int[GlobalInfo.planesNumber];
            copy.PackagesPlanes = new int[GlobalInfo.packagesNumber];

            Vans.CopyTo(copy.Vans, 0);
            PackagesVans.CopyTo(copy.PackagesVans, 0);
            Packages.CopyTo(copy.Packages, 0);
            Planes.CopyTo(copy.Planes, 0);
            PackagesPlanes.CopyTo(copy.PackagesPlanes, 0);

            return copy;
        }

    }
}
