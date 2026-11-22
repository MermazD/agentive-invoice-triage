# ADR-0003 : Exposer l'extraction OCR comme plugin Semantic Kernel

- Date de rédaction : 2026-10-12
- Statut : Reconstitué à partir de l'implémentation
- Confiance sur le choix : élevée
- Confiance sur les motivations historiques : moyenne

## Contexte

La classe est annotée comme composant Semantic Kernel : `ExtractTextFromPdfAsync` porte `[KernelFunction("ExtractTextFromPdf")]` et prend un chemin de fichier. L'agent de triage importe le plugin, invoque cette fonction, puis transmet le texte brut à l'étape LLM.

Le découpage suggère une séparation entre l'accès au document et l'interprétation métier. Le dépôt ne confirme pas si la décision visait principalement l'orchestration par modèle, la réutilisation du plugin ou une expérimentation Semantic Kernel.

## Décision

Présenter l'extraction PDF/OCR comme une fonction de plugin Semantic Kernel dont l'entrée est `pdfPath` et dont la sortie est une chaîne de texte brut. La classification et la production de la décision de facture restent en dehors de cette fonction.

## Options considérées

- Appeler directement un service .NET depuis l'agent : réduit la dépendance au framework d'orchestration, mais ne fournit pas une fonction enregistrable/invoquable comme outil Semantic Kernel.
- Retourner directement un objet facture structuré : pourrait réduire le travail d'interprétation ultérieur, mais mêlerait OCR et compréhension métier et supposerait une extraction structurée déterministe.
- Retourner des blocs OCR avec coordonnées : préserverait la disposition, mais rendrait le contrat et les consommateurs plus complexes.

Ces options sont des alternatives plausibles; leur rejet n'est pas établi par les documents.

## Conséquences

- L'agent peut appeler l'OCR via un nom de fonction stable et traiter le texte indépendamment de l'implémentation interne.
- Le plugin reste responsable du fichier local, du rendu PDF et de l'exécution native, tandis que l'interprétation du texte appartient à l'étape suivante.
- Le retour sous forme de texte brut laisse au LLM ou au parseur le soin de retrouver les champs et perd les positions, la confiance par mot et la structure des tableaux.
- Le paramètre de chemin lie l'outil à un système de fichiers accessible au processus; il ne convient pas tel quel à un agent distant ou à un service multi-tenant.
- L'annotation Semantic Kernel crée un couplage de compilation au framework et impose de conserver un contrat compatible avec son mécanisme d'invocation.
