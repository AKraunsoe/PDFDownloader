public class MenuOption
    {
        public string Name { get; }
        public Action Selected { get; }

        public MenuOption(string name, Action selected)
        {
            Name = name;
            Selected = selected;
        }
    }