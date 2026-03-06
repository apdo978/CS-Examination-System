namespace ConsoleApp2
{
    public class Teacher
    {
        private static int _idCounter;

        public string Name { get; set; }
        public string Email { get; set; }
        public int Id { get; }

        public Teacher(string name, string email)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Id = ++_idCounter;
        }

        public override string ToString() => $"Teacher: {Name} (ID: {Id}, Email: {Email})";
    }
}
