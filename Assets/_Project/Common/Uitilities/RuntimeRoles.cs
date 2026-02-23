public static class RuntimeRoles
{
    public static bool IsServer =>
#if UNITY_SERVER
        true;
#else
        false;
#endif
}