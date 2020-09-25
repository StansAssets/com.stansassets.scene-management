using System;

namespace StansAssets.SceneManagement.Build
{
    public enum BuildTargetRuntime
    {
        NoTarget = -2, // 0xFFFFFFFE
        [Obsolete("BlackBerry has been removed in 5.4")] BB10 = -1, // 0xFFFFFFFF
        [Obsolete("Use WSAPlayer instead (UnityUpgradable) -> WSAPlayer", true)] MetroPlayer = -1, // 0xFFFFFFFF
        /// <summary>
        ///   <para>OBSOLETE: Use iOS. Build an iOS player.</para>
        /// </summary>
        [Obsolete("Use iOS instead (UnityUpgradable) -> iOS", true)] iPhone = -1, // 0xFFFFFFFF
        /// <summary>
        ///   <para>Build a macOS standalone (Intel 64-bit).</para>
        /// </summary>
        StandaloneOSX = 2,
        [Obsolete("Use StandaloneOSX instead (UnityUpgradable) -> StandaloneOSX", true)] StandaloneOSXUniversal = 3,
        /// <summary>
        ///   <para>Build a macOS Intel 32-bit standalone. (This build target is deprecated)</para>
        /// </summary>
        [Obsolete("StandaloneOSXIntel has been removed in 2017.3")] StandaloneOSXIntel = 4,
        /// <summary>
        ///   <para>Build a Windows standalone.</para>
        /// </summary>
        StandaloneWindows = 5,
        /// <summary>
        ///   <para>Build a web player. (This build target is deprecated. Building for web player will no longer be supported in future versions of Unity.)</para>
        /// </summary>
        [Obsolete("WebPlayer has been removed in 5.4", true)] WebPlayer = 6,
        /// <summary>
        ///   <para>Build a streamed web player.</para>
        /// </summary>
        [Obsolete("WebPlayerStreamed has been removed in 5.4", true)] WebPlayerStreamed = 7,
        /// <summary>
        ///   <para>Build an iOS player.</para>
        /// </summary>
        iOS = 9,
        [Obsolete("PS3 has been removed in >=5.5")] PS3 = 10, // 0x0000000A
        [Obsolete("XBOX360 has been removed in 5.5")] XBOX360 = 11, // 0x0000000B
        /// <summary>
        ///   <para>Build an Android .apk standalone app.</para>
        /// </summary>
        Android = 13, // 0x0000000D
        /// <summary>
        ///   <para>Build a Linux standalone.</para>
        /// </summary>
        StandaloneLinux = 17, // 0x00000011
        /// <summary>
        ///   <para>Build a Windows 64-bit standalone.</para>
        /// </summary>
        StandaloneWindows64 = 19, // 0x00000013
        /// <summary>
        ///   <para>WebGL.</para>
        /// </summary>
        WebGL = 20, // 0x00000014
        /// <summary>
        ///   <para>Build an Windows Store Apps player.</para>
        /// </summary>
        WSAPlayer = 21, // 0x00000015
        /// <summary>
        ///   <para>Build a Linux 64-bit standalone.</para>
        /// </summary>
        StandaloneLinux64 = 24, // 0x00000018
        /// <summary>
        ///   <para>Build a Linux universal standalone.</para>
        /// </summary>
        StandaloneLinuxUniversal = 25, // 0x00000019
        [Obsolete("Use WSAPlayer with Windows Phone 8.1 selected")] WP8Player = 26, // 0x0000001A
        /// <summary>
        ///   <para>Build a macOS Intel 64-bit standalone. (This build target is deprecated)</para>
        /// </summary>
        [Obsolete("StandaloneOSXIntel64 has been removed in 2017.3")] StandaloneOSXIntel64 = 27, // 0x0000001B
        [Obsolete("BlackBerry has been removed in 5.4")] BlackBerry = 28, // 0x0000001C
        [Obsolete("Tizen has been removed in 2017.3")] Tizen = 29, // 0x0000001D
        [Obsolete("PSP2 is no longer supported as of Unity 2018.3")] PSP2 = 30, // 0x0000001E
        /// <summary>
        ///   <para>Build a PS4 Standalone.</para>
        /// </summary>
        PS4 = 31, // 0x0000001F
        [Obsolete("PSM has been removed in >= 5.3")] PSM = 32, // 0x00000020
        /// <summary>
        ///   <para>Build a Xbox One Standalone.</para>
        /// </summary>
        XboxOne = 33, // 0x00000021
        [Obsolete("SamsungTV has been removed in 2017.3")] SamsungTV = 34, // 0x00000022
        /// <summary>
        ///   <para>Build to Nintendo 3DS platform.</para>
        /// </summary>
        [Obsolete("Nintendo 3DS support is unavailable since 2018.1")] N3DS = 35, // 0x00000023
        [Obsolete("Wii U support was removed in 2018.1")] WiiU = 36, // 0x00000024
        /// <summary>
        ///   <para>Build to Apple's tvOS platform.</para>
        /// </summary>
        tvOS = 37, // 0x00000025
        /// <summary>
        ///   <para>Build a Nintendo Switch player.</para>
        /// </summary>
        Switch = 38, // 0x00000026
        Lumin = 39, // 0x00000027
    }
}
