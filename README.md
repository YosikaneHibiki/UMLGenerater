
# 使いかた
先ずはGitHubのsecretにUnity のライセンスと
パスワードとメールアドレスを登録してください。
![image](https://github.com/user-attachments/assets/98a6344c-0ec9-4e60-9267-d87db525caba)

そしてDiscodに結果を送信するためにDiscodeのWebHOOKの登録をします。

![image](https://github.com/user-attachments/assets/4c4807fb-3e1f-462a-a578-28963728a803)

そしてyamlファイルをGameProject/.github/workflows に配置してください

#### ＵＭＬGenerater側の設定
![image](https://github.com/user-attachments/assets/093a5e6b-018b-4626-aba8-fc362ab571fb)

ここのパスを解析したいフォルダーに設定して下さい

C＃クラスのUMLGeneratorをUnityのEditorファイルの中に入れてください(なければ作成する)

# Secretの登録方法

最初にリポジトリのSettingを押して
![image](https://github.com/user-attachments/assets/fea0d960-278d-41f4-a64f-3058dc965ba1)

Settingの中のSecrets and variablesを押してNew repository secret 
![image](https://github.com/user-attachments/assets/72ea557e-05e4-49ce-9391-5f86da44e115)


nameとSecretを登録してください。
![image](https://github.com/user-attachments/assets/1fda7c65-65a2-4b46-b4b9-6f371c294f1b)

WebHOOKの登録には
https://discord.com/api/webhooks/{WEBHOOK_ID}/{WEBHOOK_TOKEN}
{WEBHOOK_ID} → WebhookのID
{WEBHOOK_TOKEN} → WebhookのTOKEN
これを登録してください。

# 注意事項
このコードは勉強中ですのでUMLクラス図自体には集約やクラスの中身は存在しません
### 存在するのは
依存関係のみです

