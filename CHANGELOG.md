# Changelog

Todas as mudanças relevantes deste projeto são documentadas aqui.
Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/), com versionamento [SemVer](https://semver.org/lang/pt-BR/).

## [Unreleased]

## [0.0.2] - 2026-10-04

### Fixed
- A cena `Workshop` era salva sem a referência ao `GameDatabase` e sem os tiles do chão. O builder carregava os assets antes do `NewScene`, que os descarregava. Agora carrega depois e valida as referências antes de salvar.
- `NullReferenceException` do Input System ao apertar teclas depois de entrar no Play mais de uma vez. Era o "Enter Play Mode Options" sem recarregar o domínio, padrão do template; o domínio agora é recarregado a cada Play.
- O `Build Workshop Scene` quebrava se executado com o Play ligado. Agora o menu fica desabilitado nesse caso.

### Added
- O Editor sempre inicia o Play pela cena `Workshop` e a abre automaticamente no lugar de uma cena "Untitled" vazia.
- Entrada batch `AnvilClickerSetup.RebuildWorkshopSceneBatch` para forçar a reconstrução da cena.

## [0.0.1] - 2026-10-03 — M1: Fundação e o primeiro golpe

### Added
- Projeto Unity 6000.3.25f1 (6.3 LTS) com URP 2D Renderer, Input System, Tilemap e Test Framework.
- Assemblies separados: `Core` (C# puro), `Data`, `Runtime`, `Presentation`, `Editor` e `Tests.EditMode`.
- Core:
  - `GameState`, `Wallet` (ouro), `ForgeService` (golpe, crítico, conclusão de armas com sobra de PF) e `GameContext`;
  - `NumberFormatter` (K, M, B… `aa`, `ab`…, notação científica, separadores pt-BR).
- Dados em ScriptableObjects: `WeaponTypeDefinition`, `GameBalanceConfig`, `GameDatabase`.
- `GameBootstrap` como composition root (sem singletons) e `ForgeInput`, com a ação `Forge/Strike` (mouse na bigorna ou Espaço).
- HUD em UI Toolkit: ouro, arma atual, barra de progresso, poder do golpe e armas forjadas.
- Feedback do golpe:
  - faíscas, squash da bigorna, números flutuantes (com destaque no crítico) e flash da luz da forja;
  - screen shake no crítico;
  - sons procedurais, sem assets de áudio.
- Ferramentas em `Tools/Anvil Clicker/…`: criar pastas, configurar o projeto, gerar sprites placeholder, gerar dados de exemplo e montar a cena `Workshop`. Todas idempotentes, com entrada batch `AnvilClickerSetup.RunAllBatch`.
- Testes EditMode para `Wallet`, `ForgeService`, `GameContext` e `NumberFormatter`.
- Documentação: GDD, arquitetura, roadmap, README e CLAUDE.md.
- `.gitignore` da Unity e `.gitattributes` com Git LFS para binários.
