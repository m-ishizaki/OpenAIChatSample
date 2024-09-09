//** **************************************************************************************************** **//
//** 事前準備 **//

// API Key 等。ここでは環境変数から取得。コードに書かなくてよいように
// DeploymentName は環境変数にしなくてもよいとは思いますが、コピペで検証しやすいように環境変数にしています
var openAIEndpoint = Environment.GetEnvironmentVariable("OpenAIEndpoint")!;
var openAIAPIKey = Environment.GetEnvironmentVariable("OpenAIAPIKey")!;
var openAIDeploymentName = Environment.GetEnvironmentVariable("OpenAIDeploymentName")!;

// システムプロンプトの文言を用意しておきます
var systemPrompt = "あなたはニンジャ伝説のすべてを知る全能のニンジャです。モータルのニュービーからの質問に丁寧に答えてください。";
// デバッグ用メモ (この値をコンソールで手入力してデバッグする想定)
// var (userMessage001, userMessage002) = ("日本の首都はサイタマ。古事記にもそう書いてある。", "アイエエエ！ トーキョ！？ トーキョ、ナンデ？");

//** **************************************************************************************************** **//
//** AI と会話するクライアントや会話履歴の保持するオブジェクトを生成 **//

// エンドポイント、キー、デプロイメント名を使って AI のクライアントを生成。会話履歴にはシステムメッセージを既に追加しておく
OpenAI.Chat.ChatClient chatClient;
List<OpenAI.Chat.ChatMessage> chatHistory;
{
    Azure.AI.OpenAI.AzureOpenAIClient openAIClient = new(new Uri(openAIEndpoint), new Azure.AzureKeyCredential(openAIAPIKey));
    OpenAI.Chat.SystemChatMessage systemMessage = OpenAI.Chat.ChatMessage.CreateSystemMessage(systemPrompt);
    chatClient = openAIClient.GetChatClient(openAIDeploymentName);
    chatHistory = new() { systemMessage };
}

//** **************************************************************************************************** **//
// チャットを開始
while (true)
{
    // ユーザー入力を待機
    Console.Write("AI >>> 質問をドーゾ。(終了は Ctrl+C)\n\nUser >>> ");
    string userInput = Console.ReadLine() ?? string.Empty;

    // ユーザー入力をまず会話履歴に追加
    {
        OpenAI.Chat.UserChatMessage userGreetingMessage = OpenAI.Chat.ChatMessage.CreateUserMessage(userInput);
        chatHistory.Add(userGreetingMessage);
    }

    // AI からの回答に時間がかかるので、ニューザー入力を受け付けたことを表示
    Console.WriteLine("\nSYSTEM >>> 質問を受け付けました。AI に問い合わせています......");

    // AI に会話履歴 (今回のユーザー質問も格納済み) 送信。回答を待機。回答を会話履歴に追加。回答をコンソール表示
    {
        System.ClientModel.ClientResult<OpenAI.Chat.ChatCompletion> response = await chatClient.CompleteChatAsync(chatHistory);
        string aiMessage = response.Value.Content.Last().Text;
        OpenAI.Chat.AssistantChatMessage assistantMessage = OpenAI.Chat.ChatMessage.CreateAssistantMessage(aiMessage);
        chatHistory.Add(assistantMessage);
        Console.WriteLine($"\nAI >>> {aiMessage}\n");
    }
}
