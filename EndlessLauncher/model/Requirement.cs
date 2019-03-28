namespace EndlessLauncher.model
{
    public class Requirement
    {
        public void Resolve(bool isMet, string error)
        {
            IsMet = isMet;

            if (!isMet)
                Error = error;
            else
                Error = null;
        }

        public bool IsMet { get; private set; }

        public string Error { get; private set; }
    }
}
