# PCBuildOptimizer
The goal of this application is to put a budget (minimum and maximum) and the application will suggest the best performance per dollar for each category of computer.

## The programming languages
C#, .NET MAUI App, SQL, XAML

## The AI-driven technologies
GitHub Copilot (Claude Haiku 4.5), Chat GPT

## The programming interface
API REST

## The project

I wanted to create a project that included AI and I thought of making the project “PC Build Optimizer”. The goal of this application is to put a budget (minimum and maximum) and the application will suggest the best performance per dollar for each category of computer. The categories are : Laptop, Gaming Laptop, Tower, Gaming Tower, Mini-PC, Server and workstation. I trained 2 machine learning models. The first one is responsible to tell which computer belong to which category. It take the components of the computer to tell its answer and it use Data Classification to do so. The second machine learning model is to tell the performance per dollar for each build. It look at the performance of each component and give a result. It use “Value Prediction” to accomplish the task.

It’s also the first project that I end up using GitHub Copilot (Claude Haiku 4.5) and Chat GPT to develop the application. I used GitHub Copilot (Claude Haiku 4.5) in Visual Studio Code 2026 and Chat GPT to develop the database in SQL Server Management Studio 22.

## The challenge: Using GitHub Copilot (Claude Haiku 4.5) and Chat GPT

The problem:
The challenge was that Claude often end up suggesting a bunch of ideas of what to add and how to fix that I didn’t want. It end up creating a lot of code that I had to delete or change because it deleted the code of others function that I still needed. It was the same idea with the database and Chat GPT.

The Solution:
I had the be very precise and it has the be a small task. If I ask to much, it will do a lot but it’s not necessary for what I want and it can take longer to fix the elements in the project that I didn’t want. I also need to make sure to understand what it’s gonna add, change or delete to the code.

What I have learned:
I can depends on the AI but I can’t only expect the AI do all the work. I have to work with the AI to be able to create the project. I see the AI more like an assistant coder than a creator.

# How to use

## 1. Run the SQL Script
1. Open SQL Server Management Studio 22
2. Run the script found in the folder "SQL Script"

## 2. Run the API : PcBuilderModelAI_WebApi_ValuePrediction
1. Open a powershell Onglet
2. Run this command : cd PcBuilderModelAI_WebApi_ValuePrediction
3. Run this command : dotnet run

## 3. Run the API : MLModelDataClassification_WebApi
1. Open a powershell Onglet
2. Run this command : cd MLModelDataClassifiction_WebApi
3. Run this command : dotnet run

## 4. Run the application!
1. Run the application by pressing the run button and by pressing F5 in visual studio!