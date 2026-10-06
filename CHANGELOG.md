# Changelog

Todas as mudanças relevantes deste projeto são documentadas aqui.
Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/), com versionamento [SemVer](https://semver.org/lang/pt-BR/).

## [Unreleased]

## [0.0.3] - 2026-10-06 — M2: Economia, idle e persistência

### Added
- Economia: custo geométrico `base · crescimentoⁿ`, preço em lote pela soma geométrica e "máximo comprável" (`EconomyFormulas`).
- `ModifierStack`: 7 tipos de modificador (poder do golpe, velocidade da forja, crítico, aprendizes, valor de venda) calculados a partir dos upgrades; multiplicadores de upgrades diferentes se multiplicam.
- 9 melhorias (martelos, foles, carvão, olho do mestre, golpe certeiro, lábia de mercador, treinamento) e 4 tipos de aprendiz (Aprendiz, Jornaleiro, Veterano, Mestre). A produção de cada tipo dobra com 25, 50, 100 e 200 unidades.
- Produção passiva: `GameContext.Tick` e `GameLoop`.
- Save em JSON versionado (Newtonsoft) com migrações, escrita atômica, backup `.bak` e quarentena `.corrupt`.
- Autosave a cada 30 s e ao perder o foco, pausar ou fechar.
- Progresso offline: teto de 8 h, 50% de eficiência, mínimo de 1 min, tolerante a relógio voltando no tempo. Modal de resumo ao voltar ("Bem-vindo de volta, ferreiro!").
- Loja em UI Toolkit: abas Melhorias e Aprendizes, compra ×1/×10/×100/Máx, itens liberados por ouro acumulado e PF/s no HUD. Clicar na UI não conta como golpe.
- Dados: `UpgradeDefinition`, `ApprenticeDefinition` e `GameDatabaseValidator`.
- Ferramentas em `Tools/Anvil Clicker/`: `Data/Validate Database`, `Save/Open Save Folder`, `Save/Delete Save` e cheats em Play (`Debug/Add 1K Gold`, `Add 1M Gold`, `Simulate 1h Offline`, `Save Now`).
- Pacote `com.unity.nuget.newtonsoft-json` 3.2.2 e `link.xml` para builds IL2CPP/WebGL.
- Testes EditMode: 80 novos casos (148 no total) para fórmulas, modificadores, upgrades, aprendizes, offline, save/migração e validação dos dados reais.
- Painel de progresso do projeto em `docs/progress/`.

### Changed
- `IForgeBalance` virou `IGameBalance` e o `ForgeService` lê os números finais de `IForgeStats` (o `ModifierStack`).
- Armas prontas continuam sendo vendidas na hora, mas os ganhos aparecem agrupados num único "+ouro" a cada 0,35 s, para não poluir a tela com aprendizes.
- `Run All` roda a validação do banco de dados.

### Known issues
- Build WebGL e validação do save no IndexedDB adiados para o início do M3.
- Se um build do Player for interrompido, o cache global do Unity (`%LOCALAPPDATA%\Unity\Caches\bee`) pode ficar corrompido e os sprites aparecem magenta. Apague essa pasta e refaça o build.

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
