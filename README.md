# VCC_API

ハードウェアインターフェースを使わずに、VCClient と Voice Meeter を使って、できるだけ低遅延で可愛い声になるためのサポートアプリです。  
1PC構成でも利用可能ですが、主に2PC構成を想定しています。

VCClient v.2.0.55-alpha以降  
https://github.com/w-okada/voice-changer/tree/v.2

Voicemeeterは、VoicemeeterでもBananaでもPotatoでも、VBANが使えればどれでも使用可能です。（Virtual Audio Cable不可）  
https://vb-audio.com/Voicemeeter/

![Flowchart](Documents/Flowchart.png)

---

インストールとアンインストールについて

年月日_VCC_API.zip をダウンロードして展開し、VCC_API_Setup.exe で初期設定を済ませた後に、VCC_API.exe を実行してください。

レジストリなどは使用していませんので、アンインストールは展開したファイルを削除するだけでOKです。

---

VCC_API の設定について

![VCC_API_Settings](Documents/VCC_API_Settings.png)

おそらく変更が必要な点は3カ所です。

1. 音声入力設定: Chunk size
音声処理単位時間指定（0.05秒未満にはできません）
この数値が最も大きく遅延に影響します。
小さい方が遅延が少なくなりますが、小さくしすぎてGPUが足りなくなると、逆に遅延が大きくなります。
RTX3060Tiで0.25秒位、RTX4070Superで0.15秒位が最小です。
VCClientのベンチマークで大雑把な数値を調べることが出来ます。

1. 音声入力設定: Threshold level
音声入力の無音判定レベル指定
指定レベル以下が3秒間続くと蓄積遅延リセットします。
指定レベル以下が1分間続くと変換処理を一時停止します。
パソコン用マイク使用時は0.1くらいで使用していますが、
Questマイク使用時は0.01くらいでも大丈夫でした。
喋り終わったときの蓄積遅延リセットが働く程度に小さい値にしてください。

1. 音声変換設定: IP Address
VCClientのIPアドレス設定
2PC構成の時は、IPアドレスを調べて入力してください。
1PC構成の時は 127.0.0.1 でOKです。

---

VCClient の設定について

![VCClient_Settings](Documents/VCClient_Settings.png)

クライアントモードで正常に可愛くなれているなら、基本的にそのままで大丈夫なはずです。

---

Voice Meeter の設定について

![VoiceMeeter_Settings](Documents/VoiceMeeter_Settings.png)

Voice Meeter は、管理者権限で起動してください。
右上のVBANボタンをクリックすると、VBAN設定画面が開きます。
基本的に、スクリーンショットの通りで大丈夫だと思います。

---

Quest Link の設定について

特に設定は必要ありませんが、私はSteam版VRChatを使用していません。

---

私が使った範囲でのTips

1. WiFi接続はとても遅延が大きいです。ボイチェンを使うときだけは、Air Link や、Virtual Desktop 等を使わず、USBとLANを使った方がいいかもしれません。
1. LAN内で他のパソコンが大量にデータ送受信すると、socket.io の遅延のばらつきが大きくなります。速いときも遅いときもあるなーと思ったら、LANカードを増設してクロス…（以下略
1. 遅延よりもプチノイズや音飛びの方が、より会話を阻害します。チャンクサイズはギリギリを狙わない方が無難です。

---

履歴

2024/09/06 初版

