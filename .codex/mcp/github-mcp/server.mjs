import process from "node:process";

const GITHUB_API_BASE = "https://api.github.com";
const GITHUB_TOKEN = process.env.GITHUB_TOKEN ?? "";
const DEFAULT_OWNER = process.env.GITHUB_OWNER ?? "";
const DEFAULT_REPO = process.env.GITHUB_REPO ?? "";

if (!GITHUB_TOKEN) {
  logStderr("GITHUB_TOKEN is not set. Tool calls will fail until token is configured.");
}

let buffer = "";

process.stdin.setEncoding("utf8");
process.stdin.on("data", (chunk) => {
  buffer += chunk;
  processBuffer();
});

process.stdin.on("error", (err) => {
  logStderr(`stdin error: ${err.message}`);
});

function logStderr(message) {
  process.stderr.write(`[github-mcp] ${message}\n`);
}

function processBuffer() {
  while (true) {
    const headerEnd = buffer.indexOf("\r\n\r\n");
    if (headerEnd === -1) {
      return;
    }

    const headerRaw = buffer.slice(0, headerEnd);
    const headers = parseHeaders(headerRaw);
    const contentLengthHeader = headers["content-length"];
    if (!contentLengthHeader) {
      logStderr("Missing Content-Length header; dropping malformed frame.");
      buffer = buffer.slice(headerEnd + 4);
      continue;
    }

    const contentLength = Number(contentLengthHeader);
    if (!Number.isFinite(contentLength) || contentLength < 0) {
      logStderr("Invalid Content-Length header; dropping malformed frame.");
      buffer = buffer.slice(headerEnd + 4);
      continue;
    }

    const messageStart = headerEnd + 4;
    const messageEnd = messageStart + contentLength;
    if (buffer.length < messageEnd) {
      return;
    }

    const payload = buffer.slice(messageStart, messageEnd);
    buffer = buffer.slice(messageEnd);

    let message;
    try {
      message = JSON.parse(payload);
    } catch (err) {
      logStderr(`Invalid JSON payload: ${err.message}`);
      continue;
    }

    handleMessage(message).catch((err) => {
      logStderr(`Unhandled message error: ${err.message}`);
      if (message?.id !== undefined) {
        sendError(message.id, -32603, `Internal error: ${err.message}`);
      }
    });
  }
}

function parseHeaders(raw) {
  const headers = {};
  for (const line of raw.split("\r\n")) {
    const idx = line.indexOf(":");
    if (idx <= 0) {
      continue;
    }
    const name = line.slice(0, idx).trim().toLowerCase();
    const value = line.slice(idx + 1).trim();
    headers[name] = value;
  }
  return headers;
}

function sendMessage(message) {
  const body = JSON.stringify(message);
  const frame = `Content-Length: ${Buffer.byteLength(body, "utf8")}\r\n\r\n${body}`;
  process.stdout.write(frame);
}

function sendResult(id, result) {
  sendMessage({
    jsonrpc: "2.0",
    id,
    result
  });
}

function sendError(id, code, message) {
  sendMessage({
    jsonrpc: "2.0",
    id,
    error: {
      code,
      message
    }
  });
}

async function handleMessage(message) {
  const { id, method, params } = message;
  if (!method) {
    if (id !== undefined) {
      sendError(id, -32600, "Invalid request: method is required");
    }
    return;
  }

  switch (method) {
    case "initialize": {
      sendResult(id, {
        protocolVersion: params?.protocolVersion ?? "2025-06-18",
        capabilities: {
          tools: {
            listChanged: false
          }
        },
        serverInfo: {
          name: "github-mcp-server",
          version: "1.0.0"
        }
      });
      return;
    }
    case "notifications/initialized": {
      return;
    }
    case "tools/list": {
      sendResult(id, {
        tools: getTools()
      });
      return;
    }
    case "tools/call": {
      try {
        const toolResult = await callTool(params);
        sendResult(id, toolResult);
      } catch (err) {
        sendResult(id, {
          isError: true,
          content: [
            {
              type: "text",
              text: `Tool execution failed: ${err.message}`
            }
          ]
        });
      }
      return;
    }
    default: {
      if (id !== undefined) {
        sendError(id, -32601, `Method not found: ${method}`);
      }
    }
  }
}

function getTools() {
  return [
    {
      name: "github_get_repo",
      description: "Get metadata for a GitHub repository.",
      inputSchema: {
        type: "object",
        properties: {
          owner: { type: "string", description: "Repository owner or organization." },
          repo: { type: "string", description: "Repository name." }
        },
        additionalProperties: false
      }
    },
    {
      name: "github_list_pull_requests",
      description: "List pull requests for a repository.",
      inputSchema: {
        type: "object",
        properties: {
          owner: { type: "string", description: "Repository owner or organization." },
          repo: { type: "string", description: "Repository name." },
          state: {
            type: "string",
            enum: ["open", "closed", "all"],
            description: "Pull request state filter."
          },
          per_page: {
            type: "integer",
            minimum: 1,
            maximum: 100,
            description: "Maximum results to return."
          }
        },
        additionalProperties: false
      }
    },
    {
      name: "github_create_issue",
      description: "Create an issue in a repository.",
      inputSchema: {
        type: "object",
        properties: {
          owner: { type: "string", description: "Repository owner or organization." },
          repo: { type: "string", description: "Repository name." },
          title: { type: "string", minLength: 1, description: "Issue title." },
          body: { type: "string", description: "Issue body in Markdown." }
        },
        required: ["title"],
        additionalProperties: false
      }
    }
  ];
}

async function callTool(params) {
  const name = params?.name;
  const args = params?.arguments ?? {};
  const owner = args.owner || DEFAULT_OWNER;
  const repo = args.repo || DEFAULT_REPO;

  if (!GITHUB_TOKEN) {
    throw new Error("GITHUB_TOKEN is missing.");
  }

  if (!owner || !repo) {
    throw new Error("owner and repo are required (or set GITHUB_OWNER and GITHUB_REPO).");
  }

  switch (name) {
    case "github_get_repo": {
      const data = await githubRequest("GET", `/repos/${owner}/${repo}`);
      return toTextResult(data);
    }
    case "github_list_pull_requests": {
      const state = args.state ?? "open";
      const perPage = Number.isFinite(args.per_page) ? args.per_page : 20;
      const data = await githubRequest(
        "GET",
        `/repos/${owner}/${repo}/pulls?state=${encodeURIComponent(state)}&per_page=${encodeURIComponent(
          String(perPage)
        )}`
      );
      return toTextResult(data);
    }
    case "github_create_issue": {
      const title = typeof args.title === "string" ? args.title.trim() : "";
      if (!title) {
        throw new Error("title is required.");
      }
      const body = typeof args.body === "string" ? args.body : undefined;
      const data = await githubRequest("POST", `/repos/${owner}/${repo}/issues`, {
        title,
        body
      });
      return toTextResult(data);
    }
    default:
      throw new Error(`Unsupported tool: ${name}`);
  }
}

async function githubRequest(method, path, body) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 15000);
  try {
    const response = await fetch(`${GITHUB_API_BASE}${path}`, {
      method,
      headers: {
        Accept: "application/vnd.github+json",
        Authorization: `Bearer ${GITHUB_TOKEN}`,
        "User-Agent": "github-mcp-server",
        "Content-Type": "application/json"
      },
      body: body ? JSON.stringify(body) : undefined,
      signal: controller.signal
    });

    const text = await response.text();
    const data = safeJson(text);
    if (!response.ok) {
      const message = typeof data?.message === "string" ? data.message : `GitHub API error ${response.status}`;
      throw new Error(message);
    }
    return data;
  } finally {
    clearTimeout(timeout);
  }
}

function safeJson(text) {
  if (!text) {
    return {};
  }
  try {
    return JSON.parse(text);
  } catch {
    return { raw: text };
  }
}

function toTextResult(data) {
  return {
    content: [
      {
        type: "text",
        text: JSON.stringify(data, null, 2)
      }
    ]
  };
}
