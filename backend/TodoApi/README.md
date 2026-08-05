# Todo API Enterprise (Task Management System)

Uma API RESTful de gerenciamento de tarefas (To-Do) desenvolvida em **C# e .NET 8**. O projeto vai além de um simples CRUD, implementando padrões arquiteturais de mercado, Controle de Acesso Baseado em Perfis (RBAC), Trilha de Auditoria de Negócio e Segurança.

## Tecnologias e Padrões Utilizados

* **Framework:** .NET 8.0 / ASP.NET Core Web API
* **Linguagem:** C#
* **Banco de Dados:** SQLite (via Entity Framework Core)
* **Segurança:** Autenticação via JWT (JSON Web Token) com Blocklist (JTI) para Logout seguro.
* **Arquitetura:** Padrão Repository (Repository Pattern) e Injeção de Dependência (DI).
* **Tratamento de Erros:** Middleware Global de Tratamento de Exceções (Global Exception Handler).
* **Documentação:** Swagger UI com suporte nativo a injeção de tokens Bearer.

---

## Níveis de Permissão e Perfis de Acesso (RBAC)

O sistema possui controle rigoroso de visualização e manipulação de dados, baseado em dois perfis principais:

### Administrador (`Admin`)

* **Acesso Global:** Visualiza e gerencia todas as tarefas cadastradas no sistema, independentemente de quem as criou.
* **Gestão de Usuários:** Pode listar todos os usuários com dados completos (incluindo CPF e Role).
* **Criação Restrita:** Pode criar novos usuários diretamente. A senha temporária padrão gerada é baseada nos **4 primeiros dígitos do CPF** do usuário.
* **Promoção:** Possui o poder de promover usuários comuns a Administradores.
* **Auditoria Omnisciente:** Tem acesso irrestrito a todo o Histórico de Auditoria do sistema, podendo filtrar por data, usuário ou ação.

### Usuário Comum (`User`)

* **Acesso Isolado:** Só possui acesso (leitura, edição e exclusão) às **suas próprias tarefas**.
* **Compartilhamento:** Pode compartilhar suas tarefas com outros usuários cadastrados, utilizando o e-mail de destino.
* **Visão Compartilhada:** Acessa uma listagem exclusiva de tarefas que terceiros compartilharam com ele.
* **Privacidade de Diretório:** Ao listar usuários, vê apenas o *Nome* e *E-mail* dos demais (para fins de compartilhamento), preservando dados sensíveis como CPF.
* **Auditoria Restrita:** O histórico de auditoria exibe apenas as ações que o próprio usuário realizou, ações de terceiros aplicadas ao seu perfil ou ações em tarefas das quais é dono/convidado.

---

## Como Executar o Projeto Localmente

Siga os passos abaixo para garantir que a aplicação rode com segurança e o Banco de Dados seja populado corretamente.

**1. Clone o repositório e navegue até a pasta do projeto:**

```bash
cd backend/TodoApi

```

**2. Configure a senha do Administrador Padrão (Zero Trust):**
A API não possui credenciais hardcoded. A conta Admin é injetada no primeiro acesso ao banco de dados, utilizando as variáveis de ambiente seguras do seu `.NET`.
Execute o comando abaixo no terminal para definir a senha do admin:

```bash
dotnet user-secrets set "Admin:Password" "SUA_SENHA_AQUI"

```

**3. Limpe o banco de dados antigo (se aplicável):**
Se você fez alterações estruturais, delete o banco anterior para que o Entity Framework recrie o esquema completo com a Auditoria e a Blocklist:

```powershell
Remove-Item tasks.db* -ErrorAction SilentlyContinue

```

**4. Execute o projeto:**

```bash
dotnet run

```

A API será iniciada e o Swagger estará disponível (geralmente em `http://localhost:5xxx/swagger`). O usuário Admin será criado automaticamente com o e-mail: `admin@sistema.com` e a senha que você configurou no passo 2.

---

## Fluxo de Utilização Recomendado

1. **Autenticação:** Inicie fazendo login em `/api/Auth/login` (com o Admin ou um usuário registrado em `/api/Auth/register`).
2. **Autorização:** Copie o token retornado (sem aspas), clique no botão **"Authorize"** (Cadeado) no topo do Swagger e cole o token.
3. **Gestão de Tarefas:** Crie tarefas em `POST /api/Tasks`. A prioridade e o status iniciais são validados.
4. **Colaboração:** Para compartilhar, busque os e-mails na rota `GET /api/Users` e utilize a rota `POST /api/Tasks/{id}/share`.
5. **Auditoria:** Teste a auditoria em `GET /api/Users/audit-logs` com diferentes perfis para ver o isolamento de dados em ação.
6. **Logout:** Utilize `POST /api/Auth/logout` para inserir o JTI (ID do Token) na Blocklist, invalidando imediatamente a sessão.

---

##  Glossário de Endpoints (API Reference)

Abaixo estão detalhados os parâmetros, regras e permissões de cada rota da aplicação.

###  Autenticação (`/api/Auth`)

| Endpoint | Método | Permissão | Descrição |
| --- | --- | --- | --- |
| `/login` | `POST` | *Público* | Autentica um usuário e retorna o Token JWT válido por 8 horas. |
| `/register` | `POST` | *Público* | Auto-cadastro de novos usuários. O perfil gerado será sempre `User`. |
| `/logout` | `POST` | Autenticado | Invalida o token atual salvando seu JTI interno em uma Blocklist no banco. |

**Parâmetros de Body (Corpo):**

* `Login`: Requer `{ "email": "...", "password": "..." }`.
* `Register`: Requer `{ "name": "...", "email": "...", "cpf": "...", "password": "..." }`.

###  Usuários e Auditoria (`/api/Users`)

| Endpoint | Método | Permissão | Descrição |
| --- | --- | --- | --- |
| `/me` | `GET` | Autenticado | Retorna os dados do próprio usuário extraídos diretamente do Token JWT. |
| `/` | `GET` | Autenticado | Lista os usuários. (Admin vê lista completa; User vê apenas Nome e Email). |
| `/{id}/promote` | `PUT` | Admin | Altera a `Role` de um usuário especificado na URL (id) para `Admin`. |
| `/admin-create` | `POST` | Admin | Cria um usuário. Senha automática: 4 primeiros dígitos do CPF (sem pontuação). |
| `/me/password` | `PUT` | Autenticado | Permite ao usuário logado alterar sua própria senha. |
| `/audit-logs` | `GET` | Autenticado | Lista a trilha de auditoria. Respeita as travas de visão de cada perfil. |

**Parâmetros do `/audit-logs` (Query String):**

* `date` (DateTime): Filtra ações de um dia específico (Formato `YYYY-MM-DD`).
* `filterUserId` (Int): Filtra ações realizadas por um ID de usuário específico.
* `role` (String): Filtra logs de usuários pertencentes a um perfil (ex: `Admin`).
* `action` (String): Filtra por chave de ação (ex: `CREATE_TASK`).

### Tarefas (`/api/Tasks`)

| Endpoint | Método | Permissão | Descrição |
| --- | --- | --- | --- |
| `/` | `GET` | Autenticado | Lista tarefas com paginação. Admin vê de todos, User vê apenas as suas. |
| `/{id}` | `GET` | Autenticado | Busca uma tarefa específica pelo ID. |
| `/` | `POST` | Autenticado | Cria uma nova tarefa. A tarefa sempre inicia como `IsCompleted = false`. |
| `/{id}` | `PUT` | Autenticado | Atualiza título, descrição, status, prioridade e vencimento. Atualiza `IsCompleted` dinamicamente. |
| `/{id}` | `DELETE` | Autenticado | Realiza o *Soft Delete* da tarefa (marca o `DeletedAt`). |
| `/{id}/share` | `POST` | Autenticado | Compartilha a tarefa com o e-mail passado no corpo. O solicitante deve ser o dono da tarefa. |
| `/shared` | `GET` | Autenticado | Retorna uma lista de tarefas que outros usuários compartilharam com você. |

**Parâmetros do GET `/` (Query String):**

* `status` (String): Retorna tarefas com o status exato.
* `dueDate` (DateTime): Retorna tarefas cujo vencimento ocorra nesta data exata.
* `page` (Int, Default: 1): Número da página desejada.
* `pageSize` (Int, Default: 10): Quantidade de itens por página.

###  Enums / Domínios (`/api/Get...`)

Endpoints auxiliares sem necessidade de autenticação (Públicos), projetados para ajudar o Front-end a montar os *dropdowns* (selects) dinamicamente.

| Endpoint | Método | Descrição |
| --- | --- | --- |
| `/api/GetAllowedPriorities` | `GET` | Retorna o Array de prioridades: `["Baixo", "Médio-Baixo", "Médio", "Alto"]`. |
| `/api/GetAllowedStatuses` | `GET` | Retorna o Array de status: `["Pendente", "Em andamento", "Concluído"]`. |
| `/api/GetAuditActions` | `GET` | Retorna um Array de Objetos contendo as chaves exatas de Auditoria (`Action`) e suas respectivas descrições legíveis para humanos. |