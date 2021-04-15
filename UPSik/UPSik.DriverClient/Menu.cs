using System;
using System.Collections.Generic;

namespace UPSik.DriverClient
{
    public interface IMenu
    {
        void AddOption(MenuItem item);
        void ExecuteOption(int optionKey);
        void PrintAvailableOptions();
    }

    public class Menu : IMenu
    {
        private Dictionary<int, MenuItem> _options = new Dictionary<int, MenuItem>();

        public void AddOption(MenuItem item)
        {
            if (_options.ContainsKey(item.Key))
            {
                return;
            }
            _options.Add(item.Key, item);
        }

        public void ExecuteOption(int optionKey)
        {
            if (!_options.ContainsKey(optionKey))
            {
                Console.WriteLine("\n" + "The option you've chosen doesn't exist." + "\n");
                return;
            }
            var item = _options[optionKey];
            item.Action();
        }

        public void PrintAvailableOptions()
        {
            foreach (var option in _options)
            {
                Console.WriteLine($"{option.Key}. {option.Value.Description}");
            }
        }
    }
}
