desafio-todo/
│
├── backend/
│   └── TodoApi/                   <-- Projeto do Back-end (.NET Core)
│       ├── Controllers/           <-- Onde ficam os "endpoints" (as rotas que o front-end chama)
│       ├── Data/                  <-- Configuração do Banco de Dados (SQLite)
│       ├── Models/                <-- A estrutura dos dados (a classe "Tarefa")
│       ├── Program.cs             <-- Arquivo principal de configuração e inicialização
│       └── TodoApi.csproj         <-- Arquivo de projeto do .NET
│
└── frontend/
    ├── react-app/                 <-- Versão do Front-end em React
    ├── vue-app/                   <-- Versão do Front-end em Vue.js
    └── angular-app/               <-- Versão do Front-end em Angular