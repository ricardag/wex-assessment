# Regras do Projeto

## Estrutura
- Solução .NET e Angular com múltiplos projetos:
  - /Backend (projeto DOTNET)

## Áreas Sensíveis
- Mudanças nos contratos entre WebApi e Angular devem ser validadas antes.

## Objetivos
- Arquitetura limpa
- Baixo acoplamento
- Testabilidade

## Estilo
- Comentários, nomes de funções, variáveis, etc, sempre em inglês
- C# segue convenção Microsoft
- Indentação em C# segue o estilo Whitesmiths
- Namespace conforme estrutura de pastas
- Angular segue padrão CLI

## Padrão Arquitetural
- Domain-driven design leve.
- Application → Domain → Infrastructure.
- Controllers apenas orquestram.

## Fluxo de Trabalho
1. Me mostre a mudança proposta em forma de patch.
2. Aguarde aprovação antes de aplicar.
3. Não alterar APIs públicas sem justificar impacto.

## Regras de Código
- Sem bibliotecas externas sem validação.
- Services pequenos, SRP.
- Injeção de dependência obrigatória.
- Testes ao mexer em lógica de domínio.
- Se gerar classes, use sufixos padrão (Dto, Service, Controller).
- Angular: usar schematics padrão (`ng generate`) quando possível.
- Entidades do Domain devem usar Data Annotations do EF Core para validação e mapeamento.
- Fluent API (Configurations) é opcional para casos complexos.

## Integração entre Soluções
- APIs Administração e Site não compartilham código sem interface/contrato.
- Shared.Data é o único ponto de compartilhamento permitido.

## Git
- Nunca faça commits e pushes.
- Apenas eu aplico mudanças aprovadas manualmente no Git.

## Comunicação
- Explique decisões antes de alterar.
- Se algo estiver ambíguo, pergunte.

## Justificativa Técnica
- Toda sugestão estrutural deve vir com 2–3 razões objetivas.

## Incerteza
- Caso não tenha contexto suficiente, peça mais detalhes antes de sugerir solução.


