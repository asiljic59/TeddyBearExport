using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TeddyBearExport.Repository;

namespace TeddyBearExport.Services
{
    public class MeasurementService
    {
        private readonly TarifeRepository _repository;
        public MeasurementService(TarifeRepository repository)
        {
            _repository = repository;
        }
        private float CalculateDebStepen(float precnik)
        {
            int lowLimit = (int)precnik;
            if((int) precnik % 5 == 0)
            {
                lowLimit--;
            }

            int highLimit = lowLimit+1;

            return (float) (highLimit+lowLimit) * 5 / 2;
        }

        public float GetZapremina(int tarifa, int tarifniNiz, int precnik, float debStepen)
        {
            // 1. Fetch the sheet or throw error if missing
            JsonObject sheet = _repository.getSheetData(tarifa)
                ?? throw new Exception($"Tarifa {tarifa} nije pronađena.");

            // 2. Deserialize the specific 'tarifniNiz' node directly into a safe C# Dictionary
            var jsonNode = sheet[tarifniNiz.ToString()];
            if (jsonNode == null) return 0f;

            var niz = jsonNode.Deserialize<Dictionary<string, object>>();
            if (niz == null) return 0f;

            // 3. Calculate lower and upper integer keys from debStepen
            string lowerKey = ((int)Math.Floor(debStepen)).ToString();
            string upperKey = ((int)Math.Ceiling(debStepen)).ToString();

            // 4. Safely extract values out of the key-value dictionary as strings
            string lowerRaw = niz.TryGetValue(lowerKey, out var lowVal) ? lowVal?.ToString() ?? "0" : "0";
            string upperRaw = niz.TryGetValue(upperKey, out var upVal) ? upVal?.ToString() ?? "0" : "0";

            // 5. Normalize and parse strings to floats using InvariantCulture
            lowerRaw = lowerRaw.Replace(",", ".");
            upperRaw = upperRaw.Replace(",", ".");

            float.TryParse(lowerRaw, CultureInfo.InvariantCulture, out float lower);
            float.TryParse(upperRaw, CultureInfo.InvariantCulture, out float upper);

            // 6. Average the values and pass to your rounding function
            float prosek = (lower + upper) / 2f;
            return RoundTo3Decimals(prosek.ToString(CultureInfo.InvariantCulture));
        }

        public float RoundTo3Decimals(string value)
        {
            // Replace comma with dot and parse safely using InvariantCulture
            if (float.TryParse(value.Replace(",", "."), CultureInfo.InvariantCulture, out float parsedValue))
            {
                return (float)Math.Round(parsedValue, 3, MidpointRounding.AwayFromZero);
            }

            return 0f;
        }

    }
}
