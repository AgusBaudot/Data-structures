public struct Move
{
    public Disk Disk;
    public Tower From;
    public Tower To;

    public Move(Disk disk, Tower from, Tower to)
    {
        Disk = disk;
        From = from;
        To = to;
    }

    public override string ToString()
    {
        return $"Disk: {Disk}, From: {From}, To: {To}";
    }
}