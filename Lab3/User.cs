namespace Lab3
{
    class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public override string ToString() {
            return $"Id: {Id}, Username: {Username}";
        }
    }
}
