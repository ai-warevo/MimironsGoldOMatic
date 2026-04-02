# Action: Phase 4 - Implementation of Desktop Utility
Acting as [WPF/WinAPI Expert], please read @docs/MimironsGoldOMatic.Desktop/ReadME.md and reference @MimironsGoldOMatic.Shared.

Create the WPF Application (.NET 10) `MimironsGoldOMatic.Desktop` in `/src`.
- Use CommunityToolkit.Mvvm for the architecture.
- Implement the Win32 `PostMessage` injection logic to call `ReceiveGold` in WoW.
- Implement the 255-character chunking algorithm described in the documentation.
