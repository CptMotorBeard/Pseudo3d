public static class Road
{
    public enum Length
    {
        NONE = 0,
        SHORT = 25,
        MEDIUM = 50,
        LONG = 100
    }

    public enum Curve
    {
        RIGHT_HARD = -6,
        RIGHT_MEDIUM = -4,
        RIGHT_EASY = -2,
        NONE = default,
        LEFT_EASY = 2,
        LEFT_MEDIUM = 4,
        LEFT_HARD = 6,
    }

    public enum Hill
    {
        DOWN_HIGH = -60,
        DOWN_MEDIUM = -40,
        DOWN_LOW = -20,
        NONE = default,
        UP_LOW = 20,
        UP_MEDIUM = 40,
        UP_HIGH = 60,
    }
}