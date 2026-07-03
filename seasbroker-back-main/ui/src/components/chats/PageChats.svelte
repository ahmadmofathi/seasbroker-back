<script>
    import { pageTitle } from "@/stores/app";
    import PageWrapper from "@/components/base/PageWrapper.svelte";
    import { onMount } from "svelte";
    import ApiClient from "@/utils/ApiClient";
    import { superuser } from "@/stores/superuser.js";
    import PageSidebar from "@/components/base/PageSidebar.svelte";
    import { push, link, pop } from "svelte-spa-router";
    import { fade } from "svelte/transition";

    $pageTitle = "Chat Menu";

    export let params = {};

    let isLoading = false;

    let chats = [];
    let messages = [];
    let selectedChat = null;
    let newMessage = "";

    $: if (params.chatId) {
        fetchChatById(params.chatId);
    }

    // Fetch all chats (admins see all, ignoring participant restrictions)
    async function fetchChats() {
        chats = await ApiClient.collection("chats").getFullList();
      }
      
      // Fetch messages for a chat by ID
      async function fetchChatById(chatId) {
        if (!chatId) return;
        isLoading = true;
        try {
          const chat = await ApiClient.collection("chats").getOne(chatId);
          setSelectedChat(chat);
        } catch (error) {
            console.error("Error fetching chat messages:", error);
        } finally {
            isLoading = false;
        }
    }

    // Fetch messages for a selected chat
    async function fetchMessages(chatId) {
        messages = await ApiClient.collection("messages").getFullList({
            filter: `chatId = "${chatId}"`,
            sort: "created",
        });
    }

    // Subscribe to real-time chat updates
    function subscribeToChats() {
        ApiClient.collection("chats").subscribe("*", async (e) => {
            if (e.action === "create" || e.action === "update") {
                await fetchChats();
            }
        });
    }

    // Subscribe to real-time message updates
    function subscribeToMessages(chatId) {
        ApiClient.collection("messages").subscribe("*", async (e) => {
            if (e.action === "create" && e.record.chatId === chatId) {
                await fetchMessages(chatId);
            }
        });
    }

    // Send a message as admin
    async function sendMessage() {
        if (!selectedChat || !newMessage.trim()) return;
        await ApiClient.collection("messages").create({
            chatId: selectedChat.id,
            content: newMessage,
        });
        newMessage = "";
    }

    function setSelectedChat(chat) {
        selectedChat = chat;
        fetchMessages(chat.id);
        subscribeToMessages(chat.id);
    }

    onMount(async () => {
        await ApiClient.collection("_superusers").authRefresh();
        await fetchChats();
        subscribeToChats();

        return () => {
            ApiClient.collection("chats").unsubscribe();
            ApiClient.collection("messages").unsubscribe();
        };
    });
</script>

<PageSidebar class="settings-sidebar">
    <div class="sidebar-content">
        <div class="sidebar-title">Chats</div>
        {#each chats as chat}
            <div
                role="gridcell"
                tabindex="0"
                on:keydown={() => {
                    push(`/chats/${chat.id}`);
                }}
                on:click={() => {
                    push(`/chats/${chat.id}`);
                }}
                style="cursor: pointer; padding: 10px; background: {selectedChat?.id === chat.id
                    ? '#e0e0e0'
                    : 'white'}"
            >
                {chat.name}
            </div>
        {/each}
    </div>
</PageSidebar>

<PageWrapper>
    <header class="page-header">
        <nav class="breadcrumbs">
            <div class="breadcrumb-item">Chat Menu</div>
            {#if selectedChat}
                <div class="breadcrumb-item">{selectedChat.name}</div>
            {/if}
        </nav>
    </header>

    <div class="wrapper" in:fade={{ duration: 300 }}>
        <div class="panel">
            {#if isLoading}
                <div class="loader" />
            {:else if selectedChat}
                <h3>Messages in {selectedChat.name}</h3>
                <div class="chat-messages">
                    {#if messages.length === 0}
                        <p>No messages yet. Start the conversation!</p>
                    {:else}
                        {#each messages as message (message.id)}
                            <div
                                class="message-card {message.isAdmin
                                    ? 'own-message'
                                    : ''}"
                            >
                                <div class="message-author">{message.isAdmin? "Admin" : "User"}</div>
                                <div class="message-content">{message.content}</div>
                            </div>
                        {/each}
                    {/if}
                </div>
            {/if}
            {#if selectedChat}
                <div class="chat-input-bar">
                    <input 
                        on:keydown={(e) => {
                            if (e.key === "Enter" && !e.shiftKey) {
                                e.preventDefault();
                                sendMessage();
                            }
                        }}
                        bind:value={newMessage} placeholder="Type a message" />
                    <button on:click={sendMessage}><i class="ri-send-plane-line"></i></button>
                </div>
            {/if}
        </div>
    </div>
</PageWrapper>

<style>
    .message-card {
        max-width: 70%;
        margin: 0.3em 0;
        padding: 0.7em 1em;
        background: #dadbda;
        border-radius: 0.8em;
        box-shadow: 0 1px 2px rgba(0, 0, 0, 0.04);
        display: flex;
        flex-direction: column;
        align-items: flex-start;
        word-break: break-word;
    }

    .own-message {
        background: #dcf8c6;
        align-self: flex-end;
        margin-left: auto;
    }

    .message-author {
        font-size: 0.85em;
        color: #555;
        margin-bottom: 0.2em;
        font-weight: 500;
    }

    .message-content {
        font-size: 1em;
        color: #222;
    }
    .wrapper {
        display: flex;
        flex-direction: column;
        height: 80vh; /* or 100vh if you want full viewport */
    }
    .panel {
        display: flex;
        flex-direction: column;
        flex: 1 1 auto;
        height: 100%;
        position: relative;
    }
    .chat-messages {
        flex: 1 1 auto;
        overflow-y: auto;
        padding-bottom: 1em;
    }
    .chat-input-bar {
        display: flex;
        align-items: center;
        gap: 0.5em;
        padding: 0.5em 0;
        background: #fff;
        position: sticky;
        bottom: 0;
        z-index: 10;
        border-top: 1px solid #eee;
    }
    input {
        flex: 1 1 auto;
        padding: 8px;
        margin: 0;
    }
    button {
        padding: 4px 16px;
        height: 100%;
        border: none;
        border-radius: 5px;
        cursor: pointer;
    }
    button:hover {
        background: #e0e0e0;
    }
</style>
