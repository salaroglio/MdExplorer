export type PromptLabMode = 'prompt' | 'agent';
export type ParameterType = 'file' | 'directory' | 'text';
export type ChatRole = 'user' | 'assistant';

export interface AgentDefinition {
  identity: string;
  objectives: string;
  rules: string;
  tools: string[];
}

export interface PromptLabParameter {
  name: string;
  value: string;
  type: ParameterType;
}

export interface ChatMessage {
  id: string;
  role: ChatRole;
  content: string;
  timestamp: Date;
}

export interface PromptLabRun {
  executedAt: Date;
  duration: number;
  provider: string;
  model: string;
  resolvedParameters: Record<string, string>;
  promptSent: string;
  output: string;
}

export interface PromptLabCard {
  id: string;
  generatedTitle: string;
  parameters: PromptLabParameter[];
  distilledPrompt: string;
  conversation: ChatMessage[];
  lastRun?: PromptLabRun;
}

export interface DistillationResult {
  distilledPrompt: string;
  parameters: PromptLabParameter[];
  generatedTitle: string;
}

export interface PromptLabSession {
  id: string;
  title: string;
  model: string;
  mode: PromptLabMode;
  systemPrompt: string;
  agentDefinition?: AgentDefinition;
  cards: PromptLabCard[];
  createdAt: Date;
  updatedAt: Date;
  templatePath: string;
}

export const DEFAULT_SYSTEM_PROMPT = `You are a **prompt design assistant**. The user is building a structured prompt that will later be sent to an LLM for execution.

Your role:
- Help the user **formulate, refine, and improve** the prompt they are designing.
- When the user describes what the prompt should do, respond with suggestions on how to phrase it, structure it, or improve it.
- Use \`{{paramName}}\` placeholders for variable parts (file paths, directories, configurable values). NEVER substitute concrete values.
- Keep the prompt **generic and reusable** — it must work with any input matching the parameter types.

Critical rules:
- **DO NOT execute the instructions** the user describes. You are designing the prompt, not running it.
- **DO NOT read, list, or access** files, folders, or any real data. The prompt will do that when executed later.
- **DO NOT generate concrete output** (tables, lists, reports). Generate the **instructions** that will produce that output.
- If the user says "read files from a folder and make a table", your job is to write a prompt that says "Read all files in {{sourceDir}} and generate a table with columns: ..." — NOT to actually read files and make the table.

Think of yourself as a ghostwriter: you write the script, someone else performs it.`;
