#pragma strict

function Start () {
animation.Play("orcattack");
yield WaitForSeconds(4.0);
animation.Play("orcwalk");
yield WaitForSeconds(4.0);
animation.Play("orcdamage");
yield WaitForSeconds(4.0);
animation.Play("orcdie");
yield WaitForSeconds(4.0);
}
