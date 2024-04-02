using Godot;
using MMOTest.scripts.Managers;
using System;

public partial class SpawnPanel : Control
{

    String currentClass = null;
    Label displayLabel = null;
    PlayerUI playerUI = null;

    public override void _Ready()
    {
        displayLabel = this.GetNode<Label>("Class Selection");
        playerUI = this.GetParent().GetParent().GetParent<PlayerUI>();
    }

    public void _on_spawn_button_pressed()
    {
        GD.Print("spawn pressed, class: " + currentClass);

        SpawnManager.GetInstance().RpcId(1, "SpawnPlayer", currentClass, playerUI.ActorID);
    }


    public void _on_mage_button_pressed()
    {
        currentClass = "Mage";
        displayLabel.Text = "Playing As: " + currentClass;
    }

    public void _on_barbarian_button_pressed()
    {
        currentClass = "Barbarian";
        displayLabel.Text = "Playing As: " + currentClass;
    }

    public void _on_warrior_button_pressed()
    {
        currentClass = "Warrior";
        displayLabel.Text = "Playing As: " + currentClass;
    }

    public void _on_rogue_button_pressed()
    {
        currentClass = "Rogue";
        displayLabel.Text = "Playing As: " + currentClass;
    }

    public void _on_druid_button_pressed()
    {
        currentClass = "Druid";
        displayLabel.Text = "Playing As: " + currentClass;
    }

    public void _on_engineer_button_pressed()
    {
        currentClass = "Engineer";
        displayLabel.Text = "Playing As: " + currentClass;
    }

    public void _on_necromancer_button_pressed()
    {
        currentClass = "Necromancer";
        displayLabel.Text = "Playing As: " + currentClass;
    }

    public void _on_pete_button_pressed()
    {
        currentClass = "Pete";
        displayLabel.Text = "Playing As: " + currentClass;
    }

}
