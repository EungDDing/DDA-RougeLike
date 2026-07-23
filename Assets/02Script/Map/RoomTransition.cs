namespace DDARoguelike
{
    public static class RoomTransition
    {
        public static bool IsBusy { get; private set; }

        public static bool TryBegin()
        {
            if (IsBusy)
            {
                return false;
            }

            IsBusy = true;
            return true;
        }

        public static void End()
        {
            IsBusy = false;
        }
    }
}
