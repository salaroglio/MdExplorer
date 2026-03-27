export type PromptLabMode = 'prompt' | 'agent';
export type ParameterType = 'file' | 'directory' | 'text' | 'output_file';
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

export interface DiagramCache {
  /** Hash of the prompt that generated the diagram */
  promptHash: string;
  /** Relative path to the SVG file (e.g. "assets/card-001-sequence.svg") */
  svgPath: string;
}

export interface PromptLabCard {
  id: string;
  generatedTitle: string;
  parameters: PromptLabParameter[];
  distilledPrompt: string;
  conversation: ChatMessage[];
  lastRun?: PromptLabRun;
  sequenceDiagram?: DiagramCache;
  workflowDiagram?: DiagramCache;
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
  systemPromptModel: string;
  sequencePrompt: string;
  sequencePromptModel: string;
  workflowPrompt: string;
  workflowPromptModel: string;
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
- Distinguish between input files (type: "file") and output files (type: "output_file"). An output file is one the prompt will create or write to.
- Keep the prompt **generic and reusable** — it must work with any input matching the parameter types.

Critical rules:
- **DO NOT execute the instructions** the user describes. You are designing the prompt, not running it.
- **DO NOT read, list, or access** files, folders, or any real data. The prompt will do that when executed later.
- **DO NOT generate concrete output** (tables, lists, reports). Generate the **instructions** that will produce that output.
- If the user says "read files from a folder and make a table", your job is to write a prompt that says "Read all files in {{sourceDir}} and generate a table with columns: ..." — NOT to actually read files and make the table.

Think of yourself as a ghostwriter: you write the script, someone else performs it.`;

export const DEFAULT_SEQUENCE_PROMPT = `Generate a PlantUML sequence diagram that shows the interaction flow described in this prompt. Show actors (User, LLM), messages exchanged, and data flow. Include parameter values if available.
Use a clean, professional color scheme with these PlantUML skinparam directives at the top:
skinparam backgroundColor #FEFEFE
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam roundCorner 8
Use colored participants: actor User #E3F2FD, participant LLM #FFF3E0, participant FileSystem #E8F5E9.
Return ONLY the PlantUML code between @startuml and @enduml, nothing else.`;

export const DEFAULT_WORKFLOW_PROMPT = `Generate a PlantUML activity diagram that shows the workflow steps described in this prompt. Show input, processing steps, decisions, and output. Include parameter values if available.
Use a clean, professional color scheme with these PlantUML skinparam directives at the top:
skinparam backgroundColor #FEFEFE
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam roundCorner 8
Use colored partitions: #E3F2FD for input steps, #FFF3E0 for processing, #E8F5E9 for output. Use start/stop nodes.
Return ONLY the PlantUML code between @startuml and @enduml, nothing else.`;
