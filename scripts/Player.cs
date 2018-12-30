using Godot;
using System;

public class Player : KinematicBody
{
    private const float GRAVITY = -24.8f;
    private const float MAX_SPEED = 20f;
    private const float JUMP_SPEED = 18f;
    private const float ACCEL = 4.5f;
    private const float DEACCEL = 16f;
    private const float MAX_SLOPE_ANGLE = 2f * Mathf.Pi / 9f;
    private const float MOUSE_SENSITIVITY = 0.05f;


    private Vector3 velocity;
    private Vector3 direction;

    private Camera camera;
    private Spatial rotationHelper;

    public override void _Ready()
    {
        camera = (Camera)GetNode("Rotation_Helper/Camera");
        rotationHelper = (Spatial)GetNode("Rotation_Helper");

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public override void _Process(float delta)
    {
    }

    public override void _PhysicsProcess(float timeDelta)
    {
        process_input(timeDelta);
        process_movement(timeDelta);
    }

    private void process_input(float timeDelta)
    {
        direction = new Vector3();
        var cameraTransform = camera.GetGlobalTransform();
        var inputMovementVector = new Vector2();

        if (Input.IsActionPressed("movement_forward"))
            inputMovementVector.y += 1;
        if (Input.IsActionPressed("movement_backward"))
            inputMovementVector.y -= 1;
        if (Input.IsActionPressed("movement_left"))
            inputMovementVector.x -= 1;
        if (Input.IsActionPressed("movement_right"))
            inputMovementVector.x += 1;
        inputMovementVector = inputMovementVector.Normalized();

        direction += -cameraTransform.basis.z.Normalized() * inputMovementVector.y;
        direction += cameraTransform.basis.x.Normalized() * inputMovementVector.x;

        if (IsOnFloor())
        {
            if (Input.IsActionJustPressed("movement_jump"))
            {
                velocity.y = JUMP_SPEED;
            }
        }

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (Input.GetMouseMode() == Input.MouseMode.Visible)
                Input.SetMouseMode(Input.MouseMode.Captured);
            else
                Input.SetMouseMode(Input.MouseMode.Visible);
        }
    }

    private void process_movement(float timeDelta)
    {
        direction.y = 0;
        direction = direction.Normalized();

        velocity.y += timeDelta * GRAVITY;

        var horizontalVelocity = velocity;
        horizontalVelocity.y = 0;

        var target = direction * MAX_SPEED;

        var accel = ACCEL;
        if (direction.Dot(horizontalVelocity) == 0)
            accel = DEACCEL;

        horizontalVelocity = horizontalVelocity.LinearInterpolate(target, accel * timeDelta);
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;
        velocity = MoveAndSlide(velocity, new Vector3(0f, 1f, 0f), 0.05f, 4, MAX_SLOPE_ANGLE);
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
        {
            var mouseEvent = (InputEventMouseMotion)inputEvent;
            rotationHelper.RotateX(Mathf.Deg2Rad(mouseEvent.Relative.y * MOUSE_SENSITIVITY));
            RotateY(Mathf.Deg2Rad(mouseEvent.Relative.x * MOUSE_SENSITIVITY * -1f));

            var cameraRotation = rotationHelper.Rotation;
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, -7 * Mathf.Pi / 18f, 7 * Mathf.Pi / 18f);
            rotationHelper.Rotation = cameraRotation;
        }
    }
}
