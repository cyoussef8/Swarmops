using System;
using Activizr.Basic.Types;
using Activizr.Database;

namespace Activizr.Logic.Structure
{
    [Serializable]
    public class City : BasicCity
    {
        private City (BasicCity basic)
            : base(basic)
        {
        }

        // public ctor needed for serialization
        [Obsolete ("Do not call this function directly. It is intended only for use in serialization.", true)]
        public City(): base (0, string.Empty, 0, 0)
        {
            // this instance is NOT initalized, and intended to be used only in serialization.
        }


        public Geography Geography
        {
            get { return Geography.FromIdentity(GeographyId); } // TODO: Cache
        }

        public static City FromBasic (BasicCity basic)
        {
            return new City(basic);
        }

        public static City FromName (string cityName, int countryId)
        {
            return FromBasic(PirateDb.GetDatabaseForReading().GetCityByName(cityName, countryId));
        }
        
        public static City FromName (string cityName, string countryCode)
        {
            return FromBasic(PirateDb.GetDatabaseForReading().GetCityByName(cityName, countryCode));
        }
    }
}