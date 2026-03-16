# 💈 Barbershop Management SaaS

Uma solução moderna e completa para a gestão de barbearias, desenvolvida com uma arquitetura SaaS (Software as a Service) multi-tenancy. Este sistema oferece desde o agendamento intuitivo para o cliente até um painel administrativo poderoso para o proprietário do estabelecimento.

## 🚀 Funcionalidades

- **Multi-tenancy:** Uma única instância do software serve a múltiplas barbearias, garantindo isolamento total de dados entre clientes.
- **📅 Agendamento Inteligente:** Interface amigável para que os clientes marquem horários sem complicações.
- **💰 Controle Financeiro:** Painel para gestão de caixa, fluxo de entrada e saída.
- **🛠️ Gestão de Serviços:** Cadastro e controle de serviços oferecidos com preços e duração.
- **👨‍🔧 Painel Administrativo:** Visão geral e controle de todos os aspectos da operação da barbearia.

## 📸 Telas do Sistema

Aqui estão algumas imagens do sistema em ação:

### Dashboard Administrativo
> ![Dashboard Administrativo<img width="1919" height="1079" alt="dashboard" src="https://github.com/user-attachments/assets/c04b5472-703c-4f7e-9bbb-35878d6b305c" />
)





## 🛠️ Tecnologias Utilizadas

Este projeto é construído sobre uma base tecnológica robusta e moderna:

* **Backend:**
    * ![.NET Core](https://img.shields.io/badge/.NET%20Core-5C2D91?style=flat-square&logo=dotnet&logoColor=white)
    * ![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
    * ![Entity Framework](https://img.shields.io/badge/Entity%20Framework-512BD4?style=flat-square&logo=dotnet&logoColor=white)
* **Banco de Dados:**
    * ![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat-square&logo=microsoft-sql-server&logoColor=white)
* **Frontend:**
    * ![React](https://img.shields.io/badge/React-20232A?style=flat-square&logo=react&logoColor=61DAFB)

## 💻 Como Executar o Projeto Localmente

1.  **Clone este repositório:**
    ```bash
    git clone [https://github.com/JDS-2W/barbershop-management-saas.git](https://github.com/JDS-2W/barbershop-management-saas.git)
    ```
2.  **Configuração do Backend:**
    * Abra a pasta no terminal: `cd SistemaBarbearia`
    * Restaure os pacotes NuGet: `dotnet restore`
    * Configure a string de conexão do banco de dados no arquivo `appsettings.json`.
    * Rode as migrations para criar o banco de dados: `dotnet ef database update`
    * Inicie a aplicação: `dotnet run`

## ✉️ Contato

Desenvolvido por **Jadson Alves da Silva**. Você pode me encontrar em:

* **GitHub:** [JDS-2W](https://github.com/JDS-2W)
