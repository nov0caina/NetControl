namespace NetControl
{
    /// <summary>
    /// Categories of network devices inferred from OUI vendor and hostname heuristics.
    /// </summary>
    public enum DeviceType
    {
        Unknown,
        Router,
        Desktop,
        Mobile,
        Tablet,
        SmartTV,
        Console,
        IoT,
        Printer,
        NetworkInfra
    }
}
