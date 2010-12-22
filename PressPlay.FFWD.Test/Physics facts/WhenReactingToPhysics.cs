﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PressPlay.FFWD.Test.Core_framework;
using Box2D.XNA;

namespace PressPlay.FFWD.Test.Physics_facts
{
    [TestFixture]
    public class WhenReactingToPhysics
    {
        GameObject go;
        TestComponent component;
        TestComponent childComponent;

        [SetUp]
        public void Setup()
        {
            go = new GameObject();
            component = new TestComponent();
            go.AddComponent(component);

            GameObject child = new GameObject();
            childComponent = new TestComponent();
            child.AddComponent(childComponent);
            child.transform.parent = go.transform;
        }

        [TearDown]
        public void TearDown()
        {
            Application.AwakeNewComponents();
            Application.Reset();
        }

        #region OnTriggerEnter calls
		[Test]
        public void GameObjectWillNotCallOnTriggerEnterOnComponentsNotAwoken()
        {
            bool componentCalled = false;
            component.onTriggerEnter = () => componentCalled = true;
            go.OnTriggerEnter(null);
            Assert.That(componentCalled, Is.False);
        }

        [Test]
        public void GameObjectWillCallOnTriggerEnterOnAwokenComponents()
        {
            bool componentCalled = false;
            component.onTriggerEnter = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnTriggerEnter(null);
            Assert.That(componentCalled, Is.True);
        }

        [Test]
        public void GameObjectWillCallOnTriggerEnterOnAwokenComponentsInChildObjects()
        {
            bool componentCalled = false;
            childComponent.onTriggerEnter = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnTriggerEnter(null);
            Assert.That(componentCalled, Is.True);
        }
	    #endregion    

        #region OnTriggerExit calls
        [Test]
        public void GameObjectWillNotCallOnTriggerExitOnComponentsNotAwoken()
        {
            bool componentCalled = false;
            component.onTriggerExit = () => componentCalled = true;
            go.OnTriggerExit(null);
            Assert.That(componentCalled, Is.False);
        }

        [Test]
        public void GameObjectWillCallOnTriggerExitOnAwokenComponents()
        {
            bool componentCalled = false;
            component.onTriggerExit = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnTriggerExit(null);
            Assert.That(componentCalled, Is.True);
        }

        [Test]
        public void GameObjectWillCallOnTriggerExitOnAwokenComponentsInChildObjects()
        {
            bool componentCalled = false;
            childComponent.onTriggerExit = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnTriggerExit(null);
            Assert.That(componentCalled, Is.True);
        }
        #endregion    

        #region OnCollisionEnter calls
        [Test]
        public void GameObjectWillNotCallOnCollisionEnterOnComponentsNotAwoken()
        {
            bool componentCalled = false;
            component.onCollisionEnter = () => componentCalled = true;
            go.OnCollisionEnter(null);
            Assert.That(componentCalled, Is.False);
        }

        [Test]
        public void GameObjectWillCallOnCollisionEnterOnAwokenComponents()
        {
            bool componentCalled = false;
            component.onCollisionEnter = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnCollisionEnter(null);
            Assert.That(componentCalled, Is.True);
        }

        [Test]
        public void GameObjectWillCallOnCollisionEnterOnAwokenComponentsInChildObjects()
        {
            bool componentCalled = false;
            childComponent.onCollisionEnter = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnCollisionEnter(null);
            Assert.That(componentCalled, Is.True);
        }
        #endregion

        #region OnCollisionExit calls
        [Test]
        public void GameObjectWillNotCallOnCollisionExitOnComponentsNotAwoken()
        {
            bool componentCalled = false;
            component.onCollisionExit = () => componentCalled = true;
            go.OnCollisionExit(null);
            Assert.That(componentCalled, Is.False);
        }

        [Test]
        public void GameObjectWillCallOnCollisionExitOnAwokenComponents()
        {
            bool componentCalled = false;
            component.onCollisionExit = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnCollisionExit(null);
            Assert.That(componentCalled, Is.True);
        }

        [Test]
        public void GameObjectWillCallOnCollisionExitOnAwokenComponentsInChildObjects()
        {
            bool componentCalled = false;
            childComponent.onCollisionExit = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnCollisionExit(null);
            Assert.That(componentCalled, Is.True);
        }
        #endregion

        #region OnPreSolve calls
        [Test]
        public void GameObjectWillNotCallOnPreSolveOnComponentsNotAwoken()
        {
            bool componentCalled = false;
            component.onPreSolve = () => componentCalled = true;
            go.OnPreSolve(null, new Manifold());
            Assert.That(componentCalled, Is.False);
        }

        [Test]
        public void GameObjectWillCallOnPreSolveOnAwokenComponents()
        {
            bool componentCalled = false;
            component.onPreSolve = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnPreSolve(null, new Manifold());
            Assert.That(componentCalled, Is.True);
        }

        [Test]
        public void GameObjectWillCallOnPreSolveOnAwokenComponentsInChildObjects()
        {
            bool componentCalled = false;
            childComponent.onPreSolve = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnPreSolve(null, new Manifold());
            Assert.That(componentCalled, Is.True);
        }
        #endregion

        #region OnPostSolve calls
        [Test]
        public void GameObjectWillNotCallOnPostSolveOnComponentsNotAwoken()
        {
            bool componentCalled = false;
            component.onPostSolve = () => componentCalled = true;
            go.OnPostSolve(null, new ContactImpulse());
            Assert.That(componentCalled, Is.False);
        }

        [Test]
        public void GameObjectWillCallOnPostSolveOnAwokenComponents()
        {
            bool componentCalled = false;
            component.onPostSolve = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnPostSolve(null, new ContactImpulse());
            Assert.That(componentCalled, Is.True);
        }

        [Test]
        public void GameObjectWillCallOnPostSolveOnAwokenComponentsInChildObjects()
        {
            bool componentCalled = false;
            childComponent.onPostSolve = () => componentCalled = true;
            Application.AwakeNewComponents();
            go.OnPostSolve(null, new ContactImpulse());
            Assert.That(componentCalled, Is.True);
        }
        #endregion

    }
}
