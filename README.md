# TownOfHost-H

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

[本家TownOfHostのreadme](https://github.com/tukasa0001/TownOfHost/blob/main/README.md#town-of-host)
- 公開ルームは封印
- 知人以外からの質問やバグ報告等は[Issues](https://github.com/Hyz-sui/TownOfHost-H/issues/new)で**のみ**受け付けています  
  TwitterやDiscord等では受け付けていません
  - 既存の説明を読めばわかるもの，低レベルなもの，日本語のコミュニケーションが怪しいもの，聞く相手を間違ってるもの等々には基本対応しません
- 11人2狼村(1イビルハッカー1シェリフ1黒猫マッドスニッチ)以外の役職構成や特定の設定の組み合わせで発生するバグ等については低優先度で扱います
- 多言語対応はしていませんし，する予定もありません

## 本家との変更点

### 追加役職

#### EvilHacker/イビルハッカー

インポスター強化役職  
相方生存中，相方の方向を指す矢印が名前に表示される  
毎会議開始時にチャットに最終のアドミン情報が送られる  
インポスターがいる部屋には★印がつく  
死体のある部屋には死体の数が表記される  
他のインポスターがキルを行うとキルフラッシュが見え，名前の3行目にキルの発生した部屋が10秒間通知される

- オプション
  - 死体位置がわかる - アドミン情報で死体の数を表示するかどうか  
    デフォルトON
  - 他のインポスターの位置がわかる - アドミン情報で★印を表示するかどうか  
    デフォルトON
  - インポスターキル時にキルフラッシュが見える  
    デフォルトON
    - キルの発生場所がわかる - 能力の性質上キルフラのオプションがONの場合しか設定できません  
      デフォルトON
  - 他のインポスターを指す矢印が見える - 相方の方向を指す矢印が見えるかどうか  
    デフォルトON
  - 死亡時､生存インポスターに能力を引き継ぐ - ONにすると自分が死亡したら素インポスターに能力を引き継ぐ  
    デフォルトON
  - アドミン情報で誰もいない部屋を省略する - ONにすると，会議時のアドミン情報で無人の部屋は省略して表示されます  
    デフォルトOFF

チャットに送信されるアドミン情報の例

```text
コックピット: 2
★武器庫: 1
通信室: 1(死体×1)
★エンジンルーム: 2(死体×1)
...
```

アイデア元: [~~haoming37/TheOtherRoles-GM-Haoming~~](https://github.com/haoming37/TheOtherRoles-GM-Haoming) [haoming37/GMH](https://github.com/haoming37/GMH)，[tomarai/TheOtherRoles](https://github.com/tomarai/TheOtherRoles/tree/dev-v3.4.x)

### 追加機能

#### カメラの時間制限

デバイス無効化の子オプション｢カメラの時間制限(全マップ)｣を追加  
全体でカメラを使用できる時間を制限できる  
会議開始時にチャットで全員に向けて残り時間が通知される  
秒数の更新は追放画面の終了時に行われるため，消費された直後の会議のチャット通知では反映されない(その次の会議で秒数が減る)  
全部使い切るとカメラ画面がコミュサボ状態になり，Mod導入者はその画面を閉じると以後開けなくなる(バニラ参加者は開かないように)  
Mod視点では前を通っただけで壊れていることがわかってしまうので，バニラ参加者が壊れたカメラの前を通ると名前の3行目に｢使用不可｣と通知されるようにしている

> **Note**
> 他デバイスの制限は現時点で技術的に不可能

#### マッド系役職のオプション追加

##### 狂信タスクの易化

マッドスニッチに子オプション｢必要なタスク数を指定する｣を追加(デフォルトOFF)  
これを指定すると，例えば｢マッドスニッチにはタスクが5個配られるが，そのうちどれか1個を終わらせれば狂信能力が発動する｣というようなことが可能

##### 配電盤事故の防止

マッド系共通オプション内，サボタージュ修復可否の設定項目の下に｢Mod導入済マッド系プレイヤーは開けない｣を追加  
ONにすると，**Mod導入者のみ**，直せないよう設定されたサボタージュの画面は開けなくなる  
主にマッドメイトが配電盤を見ることができないレギュで間違って見てしまう事故の防止用

![madmate_unusable.png](./Images/madmate_unusable.png)

#### 自動部屋コードコピー

部屋建て時にコードをコピーする  
設定から無効化可能

#### Discordとの連携

試合終了時に試合結果をDiscordに送信する機能

##### 手順

1. Modを導入した状態で一度アモアスを起動すると`Among Us.exe`と同じ階層に`WebhookUrl.txt`が生成される
2. これの中身を全部消して，DiscordのWebhook(チャンネル設定→連携サービス→ウェブフックを作成)のURLを貼り付けて保存
3. アモアスの歯車の設定から｢Discordに試合結果を送信｣をONにする

廃村時や自身がホストでないとき，URLが正常に設定されていないときは何も送信しない

#### ロビーでの試合結果表示

歯車の設定から｢ロビーで前の試合の結果を表示｣をONにすると，試合終了時に左上に出てくる試合結果をロビーに戻った後でも確認することができる  
マウスホイール操作で2試合以上前の結果も確認できる

![lobby_summary.png](./Images/lobby_summary.png)

#### 憑依の改善

Mod導入者向けに憑依のパフォーマンスを改善

* 憑依開始時のデフォルトのフィルタリングを生存プレイヤーに設定(Mod設定で無効化可)
* 憑依画面で表示される憑依対象の役職がModロールになるよう変更

### 変更点

* 黒猫レギュ向けにオプションのデフォルト値を調整

## CREDITS&THANKS

* [tukasa0001/TownOfHost](https://github.com/tukasa0001/TownOfHost) - fork元
* [BepInEx](https://github.com/BepInEx)
* [tomarai/TheOtherRoles](https://github.com/tomarai/TheOtherRoles/tree/dev-v3.4.x) - EvilHackerの役職および能力のアイデア元
* [~~haoming37/TheOtherRoles-GM-Haoming~~](https://github.com/haoming37/TheOtherRoles-GM-Haoming) [haoming37/GMH](https://github.com/haoming37/GMH) - EvilHackerの能力のアイデア元
