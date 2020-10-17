namespace maple
{
    public static class Commands
    {

        static bool isInputTarget = false;

        public static void ToggleInputTarget()
        {
            isInputTarget = !isInputTarget;
        }

        public static bool IsCommandInput()
        {
            return isInputTarget;
        }

    }
}